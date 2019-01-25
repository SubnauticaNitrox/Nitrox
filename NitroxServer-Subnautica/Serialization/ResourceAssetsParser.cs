using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using NitroxModel.DataStructures.Util;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers;
using NitroxServer.GameLogic;
using NitroxServer.UnityStubs;
using UWE;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.DataStructures.GameLogic.Entities;

namespace NitroxServer_Subnautica.Serialization
{
    public static class ResourceAssetsParser
    {
        private static Dictionary<string, List<UwePrefab>> gameObjectsByClassId = new Dictionary<string, List<UwePrefab>>();
        private static AssetsFileInstance instance;
        private static AssetsManager manager;

        private static HashSet<long> checkedGameObjects = new HashSet<long>();
        private static HashSet<ulong> matchingMonoscripts = new HashSet<ulong>();

        private const uint TEXT_CLASS_ID = 0x31;
        private const uint MONOBEHAVIOUR_CLASS_ID = 0x72;

        public static ResourceAssets Parse()
        {
            ResourceAssets resourceAssets = new ResourceAssets();

            SetupAssetManager();

            AssetsFileReader resReader = new AssetsFileReader(instance.stream);
            AssetsFile resourcesFile = instance.file;
            AssetsFileReader afr = resourcesFile.reader;
            AssetsFileTable resourcesFileTable = instance.table;

            int timesRan = 0;
            foreach (AssetFileInfoEx afi in resourcesFileTable.pAssetFileInfo.Values)
            {
                if (afi.curFileType == TEXT_CLASS_ID)
                {
                    resourcesFile.reader.Position = afi.absoluteFilePos;
                    string assetName = resourcesFile.reader.ReadCountStringInt32();
                    if (assetName == "EntityDistributions")
                    {
                        resourcesFile.reader.Align();
                        resourceAssets.LootDistributionsJson = resourcesFile.reader.ReadCountStringInt32().Replace("\\n", "");
                    }
                }
                else if (afi.curFileType == MONOBEHAVIOUR_CLASS_ID)
                {
                    resourcesFile.reader.Position = afi.absoluteFilePos;
                    int gameObjectFileId = resourcesFile.reader.ReadInt32();
                    long gameObjectPathId = resourcesFile.reader.ReadInt64();

                    resourcesFile.reader.Position += 16;
                    string assetName = resourcesFile.reader.ReadCountStringInt32();
                    switch (assetName)
                    {
                        case "WorldEntityData":
                            resourcesFile.reader.Align();
                            uint size = resourcesFile.reader.ReadUInt32();
                            WorldEntityInfo wei;
                            for (int i = 0; i < size; i++)
                            {
                                wei = new WorldEntityInfo();
                                wei.classId = resourcesFile.reader.ReadCountStringInt32();
                                wei.techType = (TechType)resourcesFile.reader.ReadInt32();
                                wei.slotType = (EntitySlot.Type)resourcesFile.reader.ReadInt32();
                                wei.prefabZUp = resourcesFile.reader.ReadBoolean();
                                resourcesFile.reader.Align();
                                wei.cellLevel = (LargeWorldEntity.CellLevel)resourcesFile.reader.ReadInt32();
                                wei.localScale = new UnityEngine.Vector3(resourcesFile.reader.ReadSingle(), resourcesFile.reader.ReadSingle(), resourcesFile.reader.ReadSingle());
                                resourceAssets.WorldEntitiesByClassId.Add(wei.classId, wei);
                            }
                            break;
                        case "ReefbackSlotsData":
                            resourcesFile.reader.Align();
                            uint reefbackPlantsDataSize = resourcesFile.reader.ReadUInt32();
                            for (int i = 0; i < reefbackPlantsDataSize; i++)
                            {
                                resourcesFile.reader.ReadCountStringInt32();
                                resourcesFile.reader.Align();
                                uint plantsDataSize = resourcesFile.reader.ReadUInt32();
                                List<ReefbackSpawnData.ReefbackPlant> plantPrefabs = new List<ReefbackSpawnData.ReefbackPlant>();

                                for (int p = 0; p < plantsDataSize; p++)
                                {
                                    uint fileId = (uint)resourcesFile.reader.ReadInt32();
                                    ulong pathId = (ulong)resourcesFile.reader.ReadInt64(); // PathId

                                    AssetFileInfoEx asset = FindComponentOfType<PrefabIdentifier>(instance, pathId);
                                    ulong prevPos = resourcesFile.reader.Position;

                                    resourcesFile.reader.Position = asset.absoluteFilePos + 28;

                                    resourcesFile.reader.ReadCountStringInt32(); // Empty name
                                    string classId = resourcesFile.reader.ReadCountStringInt32(); // ClassId

                                    resourcesFile.reader.Position = prevPos;
                                    asset = FindTransform(instance, pathId);
                                    prevPos = resourcesFile.reader.Position;
                                    resourcesFile.reader.Position = asset.absoluteFilePos + 12;

                                    Quaternion rotation = new Quaternion(
                                        resourcesFile.reader.ReadSingle(), // Quaternion X
                                        resourcesFile.reader.ReadSingle(), // Quaternion Y
                                        resourcesFile.reader.ReadSingle(), // Quaternion Z
                                        resourcesFile.reader.ReadSingle()); // Quaternion W

                                    Vector3 position = new Vector3(
                                        resourcesFile.reader.ReadSingle(), // Position X
                                        resourcesFile.reader.ReadSingle(), // Position Y
                                        resourcesFile.reader.ReadSingle()); // Position Z

                                    Vector3 scale = new Vector3(
                                        resourcesFile.reader.ReadSingle(), // Scale X
                                        resourcesFile.reader.ReadSingle(), // Scale Y
                                        resourcesFile.reader.ReadSingle()); // Scale Z

                                    resourcesFile.reader.Position = prevPos;

                                    plantPrefabs.Add(new ReefbackSpawnData.ReefbackPlant()
                                    {
                                        classId = classId,
                                        position = position,
                                        scale = scale,
                                        rotation = rotation
                                    });
                                }

                                float probability = resourcesFile.reader.ReadSingle();
                                float x = resourcesFile.reader.ReadSingle();
                                float y = resourcesFile.reader.ReadSingle();
                                float z = resourcesFile.reader.ReadSingle();

                                foreach (ReefbackSpawnData.ReefbackPlant plant in plantPrefabs)
                                {
                                    ReefbackSpawnData.SpawnablePlants.Add(new ReefbackSpawnData.ReefbackPlant
                                    {
                                        classId = plant.classId,
                                        probability = probability,
                                        rotation = plant.rotation,
                                        position = plant.position,
                                        scale = plant.scale
                                    });
                                }
                            }
                            
                            uint reefbackCreatureSize = resourcesFile.reader.ReadUInt32();
                            for (int i = 0; i < reefbackCreatureSize; i++)
                            {

                                resourcesFile.reader.ReadCountStringInt32(); // Name
                                resourcesFile.reader.Align();
                                uint fileId = (uint)resourcesFile.reader.ReadInt32();
                                ulong pathId = (ulong)resourcesFile.reader.ReadInt64(); // PathId
                                ReefbackSpawnData.ReefbackCreature creaturePrefab = new ReefbackSpawnData.ReefbackCreature();
                                AssetFileInfoEx asset = FindComponentOfType<PrefabIdentifier>(instance, pathId);
                                ulong prevPos = resourcesFile.reader.Position;
                                resourcesFile.reader.Position = asset.absoluteFilePos + 28;

                                resourcesFile.reader.ReadCountStringInt32(); // Empty name
                                string classId = resourcesFile.reader.ReadCountStringInt32(); // ClassId

                                resourcesFile.reader.Position = prevPos;
                                asset = FindTransform(instance, pathId);
                                prevPos = resourcesFile.reader.Position;
                                resourcesFile.reader.Position = asset.absoluteFilePos + 12;

                                Quaternion rotation = new Quaternion(
                                    resourcesFile.reader.ReadSingle(), // Quaternion X
                                    resourcesFile.reader.ReadSingle(), // Quaternion Y
                                    resourcesFile.reader.ReadSingle(), // Quaternion Z
                                    resourcesFile.reader.ReadSingle()); // Quaternion W

                                Vector3 position = new Vector3(
                                    resourcesFile.reader.ReadSingle(), // Position X
                                    resourcesFile.reader.ReadSingle(), // Position Y
                                    resourcesFile.reader.ReadSingle()); // Position Z

                                Vector3 scale = new Vector3(
                                    resourcesFile.reader.ReadSingle(), // Scale X
                                    resourcesFile.reader.ReadSingle(), // Scale Y
                                    resourcesFile.reader.ReadSingle()); // Scale Z

                                creaturePrefab = new ReefbackSpawnData.ReefbackCreature
                                {
                                    classId = classId,
                                    position = position,
                                    scale = scale,
                                    rotation = rotation,
                                };

                                resourcesFile.reader.Position = prevPos;


                                int minNumber = resourcesFile.reader.ReadInt32();
                                int maxNumber = resourcesFile.reader.ReadInt32();
                                float probability = resourcesFile.reader.ReadSingle();

                                ReefbackSpawnData.SpawnableCreatures.Add(new ReefbackSpawnData.ReefbackCreature()
                                {
                                    classId = creaturePrefab.classId,
                                    position = creaturePrefab.position,
                                    scale = creaturePrefab.scale,
                                    rotation = creaturePrefab.rotation,
                                    probability = probability,
                                    minNumber = minNumber,
                                    maxNumber = maxNumber
                                });
                            }
                            break;
                        default:
                            if (gameObjectPathId == 0)
                            {
                                break; // Gotta love early breaks
                            }
                            else if (gameObjectPathId != 0 && !checkedGameObjects.Contains(gameObjectPathId))
                            {
                                checkedGameObjects.Add(gameObjectPathId);
                                AssetFileInfoEx asset = FindComponentOfType<PrefabPlaceholdersGroup>(instance, (ulong)gameObjectPathId);

                                if (asset != null)
                                {
                                    ulong prevPos = resourcesFile.reader.Position;
                                    resourcesFile.reader.Position = asset.absoluteFilePos + 32;

                                    int placeholders = resourcesFile.reader.ReadInt32(); //Array size
                                    List<UwePrefab> prefabs = new List<UwePrefab>();
                                    for (int a = 0; a < placeholders; a++)
                                    {
                                        resourcesFile.reader.ReadInt32();//FileId
                                        ulong pathId = (ulong)resourcesFile.reader.ReadInt64();//PathId
                                        AssetFileInfoEx prefabPlaceholder = resourcesFileTable.getAssetInfo(pathId);
                                        ulong placeholderPos = resourcesFile.reader.Position;
                                        resourcesFile.reader.Position = prefabPlaceholder.absoluteFilePos;
                                        resourcesFile.reader.ReadInt32();
                                        ulong gameObjectPath = (ulong)resourcesFile.reader.ReadInt64();
                                        AssetFileInfoEx transformAsset = FindTransform(instance, gameObjectPath);

                                        ulong curPos = resourcesFile.reader.Position;
                                        resourcesFile.reader.Position = transformAsset.absoluteFilePos + 12;

                                        Quaternion rotation = new Quaternion(
                                            afr.ReadSingle(), // Quaternion X
                                            afr.ReadSingle(), // Quaternion Y
                                            afr.ReadSingle(), // Quaternion Z
                                            afr.ReadSingle()); // Quaternion W

                                        Vector3 position = new Vector3(
                                            afr.ReadSingle(), // Position X
                                            afr.ReadSingle(), // Position Y
                                            afr.ReadSingle()); // Position Z

                                        Vector3 scale = new Vector3(
                                            afr.ReadSingle(), // Scale X
                                            afr.ReadSingle(), // Scale Y
                                            afr.ReadSingle()); // Scale Z

                                        resourcesFile.reader.Position = curPos;
                                        resourcesFile.reader.Position += 16;

                                        resourcesFile.reader.ReadCountStringInt32();//Empty...
                                        string prefabString = resourcesFile.reader.ReadCountStringInt32();

                                        prefabs.Add(new UwePrefab(prefabString, position, rotation, scale));
                                        resourcesFile.reader.Position = placeholderPos; // Reset position
                                    }

                                    AssetFileInfoEx prefabIdentifierAsset = FindComponentOfType<PrefabIdentifier>(instance, (ulong)gameObjectPathId);
                                    resourcesFile.reader.Position = prefabIdentifierAsset.absoluteFilePos + 28;

                                    resourcesFile.reader.ReadCountStringInt32(); // Empty name
                                    string classId = resourcesFile.reader.ReadCountStringInt32(); // ClassId

                                    List<UwePrefab> val;
                                    if (!gameObjectsByClassId.TryGetValue(classId, out val)) // should never be false but just incase
                                    {
                                        gameObjectsByClassId.Add(classId, prefabs);
                                    }

                                    resourcesFile.reader.Position = prevPos; // Reset position
                                    break;
                                }
                            }
                            break;

                    }
                }
            }
            Validate.IsTrue(resourceAssets.WorldEntitiesByClassId.Count > 0);
            Validate.IsTrue(resourceAssets.LootDistributionsJson != "");

            return resourceAssets;
        }

        private static AssetFileInfoEx FindComponentOfType<T>(AssetsFileInstance assetsFileInstance, ulong gameObjectPathId)
        {
            return FindComponentOfType(typeof(T), assetsFileInstance, gameObjectPathId);
        }

        private static AssetFileInfoEx FindTransform(AssetsFileInstance assetsFileInstance, ulong gameObjectPathId)
        {
            AssetsFileTable assetsTable = assetsFileInstance.table;
            AssetsFile assetsFile = assetsFileInstance.file;
            AssetsFileReader afr = assetsFile.reader;

            ulong prevPos = assetsFile.reader.Position;
            AssetFileInfoEx gameObjectAFI = assetsTable.getAssetInfo(gameObjectPathId);
            if (gameObjectAFI.curFileType != 1)
            {
                Log.Info("An incorrect pathId was passed to FindTransform please check if it points to a proper gameObject: " + gameObjectPathId);
                return null;
            }

            afr.Position = gameObjectAFI.absoluteFilePos;
            afr.Align();

            uint componentCount = afr.ReadUInt32();
            AssetFileInfoEx componentAFI;

            for (int i = 0; i < componentCount; i++)
            {
                uint fileId = (uint)afr.ReadInt32();
                ulong pathId = (ulong)afr.ReadInt64();
                if (fileId == 0)
                {
                    componentAFI = assetsTable.getAssetInfo(pathId);
                    if (componentAFI.curFileType == 4) // Ey look we got it
                    {
                        afr.Position = prevPos;
                        return componentAFI;
                    }
                }
                else
                {
                    AssetsFileInstance dep = assetsFileInstance.dependencies[(int)fileId - 1];
                    componentAFI = dep.table.getAssetInfo(pathId);
                    if (componentAFI.curFileType == 4) // Ey look we got it
                    {
                        afr.Position = prevPos;
                        return componentAFI;
                    }
                }
            }



            return null; // This should likely never happen
        }

        private static AssetFileInfoEx FindComponentOfType(Type type, AssetsFileInstance assetsFileInstance, ulong gameObjectPathId)
        {
            AssetsFileTable assetsTable = assetsFileInstance.table;
            AssetsFile assetsFile = assetsFileInstance.file;
            AssetsFileReader afr = assetsFile.reader;

            ulong prevPos = assetsFile.reader.Position;
            AssetFileInfoEx gameObjectAFI = assetsTable.getAssetInfo(gameObjectPathId);
            if (gameObjectAFI.curFileType != 1)
            {
                Log.Info("An incorrect pathId was passed to FindComponentOfType please check if it points to a proper gameObject: " + gameObjectPathId);
                return null;
            }
            afr.Position = gameObjectAFI.absoluteFilePos;
            afr.Align();

            uint componentCount = afr.ReadUInt32();
            AssetFileInfoEx componentAFI;

            for (int i = 0; i < componentCount; i++)
            {
                uint fileId = (uint)afr.ReadInt32();
                ulong pathId = (uint)afr.ReadInt64();
                if (fileId == 0)
                {
                    componentAFI = assetsTable.getAssetInfo(pathId);
                    if (componentAFI.curFileType == 114)
                    {
                        ulong curPos = afr.Position;
                        afr.Position = componentAFI.absoluteFilePos;
                        afr.Align();
                        afr.Position += 16;

                        int scriptFileId = afr.ReadInt32();
                        long scriptPathId = afr.ReadInt64();

                        if (matchingMonoscripts.Contains((ulong)scriptPathId))
                        {
                            return componentAFI;
                        }

                        AssetsFileInstance monoscriptDep = assetsFileInstance.dependencies[scriptFileId - 1];
                        AssetFileInfoEx monoscriptAFI = monoscriptDep.table.getAssetInfo((ulong)scriptPathId);
                        if (monoscriptAFI.curFileType == 115)
                        {
                            monoscriptDep.file.reader.Position = monoscriptAFI.absoluteFilePos;
                            monoscriptDep.file.reader.Align();

                            string monoscriptName = monoscriptDep.file.reader.ReadCountStringInt32();

                            if (monoscriptName == type.Name)
                            {
                                afr.Position = prevPos;
                                return componentAFI;
                            }
                        }
                        afr.Position = curPos;
                    }
                }
            }

            afr.Position = prevPos;

            return null;
        }

        public static List<UwePrefab> GetPrefabFromPlaceholder(string classId)
        {
            List<UwePrefab> prefabs;

            gameObjectsByClassId.TryGetValue(classId, out prefabs);

            if (prefabs == null)
            {
                prefabs = new List<UwePrefab>();
            }

            return prefabs;
        }

        private static Transform GetWorldTransform(AssetsFileInstance assetsFileInstance, ulong gameObjectPathId)
        {
            AssetsFileTable assetsTable = assetsFileInstance.table;
            AssetsFile assetsFile = assetsFileInstance.file;
            AssetsFileReader afr = assetsFile.reader;

            ulong prevPos = assetsFile.reader.Position;

            AssetFileInfoEx transformAsset = FindTransform(assetsFileInstance, gameObjectPathId);
            afr.Position = transformAsset.absoluteFilePos + 12;

            Quaternion rotation = new Quaternion(
                afr.ReadSingle(), // Quaternion X
                afr.ReadSingle(), // Quaternion Y
                afr.ReadSingle(), // Quaternion Z
                afr.ReadSingle()); // Quaternion W

            Vector3 position = new Vector3(
                afr.ReadSingle(), // Position X
                afr.ReadSingle(), // Position Y
                afr.ReadSingle()); // Position Z

            Vector3 scale = new Vector3(
                afr.ReadSingle(), // Scale X
                afr.ReadSingle(), // Scale Y
                afr.ReadSingle()); // Scale Z

            Transform transform = new Transform(position, scale, rotation); // establish our first transform
            int childrenCount = afr.ReadInt32();
            for (int i = 0; i < childrenCount; i++)
            {
                afr.Position += 12; //skip Children file and path ids
            }
            afr.ReadInt32();
            ulong transformPathId = (ulong)afr.ReadInt64();

            AssetFileInfoEx transformFatherAsset = assetsTable.getAssetInfo(transformPathId);
            while (transformPathId > 0)
            {
                ulong loopPos = assetsFile.reader.Position;
                afr.Position = transformFatherAsset.absoluteFilePos + 12;

                rotation = new Quaternion(
                    afr.ReadSingle(), // Quaternion X
                    afr.ReadSingle(), // Quaternion Y
                    afr.ReadSingle(), // Quaternion Z
                    afr.ReadSingle()); // Quaternion W

                position = new Vector3(
                    afr.ReadSingle(), // Position X
                    afr.ReadSingle(), // Position Y
                    afr.ReadSingle()); // Position Z

                scale = new Vector3(
                    afr.ReadSingle(), // Scale X
                    afr.ReadSingle(), // Scale Y
                    afr.ReadSingle()); // Scale Z

                transform.Position += position;

                childrenCount = afr.ReadInt32();
                for (int i = 0; i < childrenCount; i++)
                {
                    afr.Position += 12; //skip Children file and path ids
                }

                afr.ReadInt32();
                transformPathId = (ulong)afr.ReadInt64();

                transformFatherAsset = assetsTable.getAssetInfo(transformPathId);
            }
            return transform; // This should likely never happen
        }

        private static void SetupAssetManager()
        {
            manager = new AssetsManager();
            instance = manager.LoadAssetsFile(new FileStream(FindPath(), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite), true);
        }

        private static string FindAssemblyPath(Assembly assembly)
        {
            List<string> errors = new List<string>();
            Optional<string> subnauticaPath = GameInstallationFinder.Instance.FindGame(errors);

            if (subnauticaPath.IsEmpty())
            {
                Log.Info($"Could not locate Subnautica installation directory: {Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
                return assembly.FullName;
            }

            return Path.Combine(subnauticaPath.Get(), "SubnauticaData", "Managed", assembly.FullName);
        }

        private static string FindPath()
        {
            List<string> errors = new List<string>();
            Optional<string> subnauticaPath = GameInstallationFinder.Instance.FindGame(errors);
            if (subnauticaPath.IsEmpty())
            {
                Log.Info($"Could not locate Subnautica installation directory: {Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
            
            string gameResourcesPath = "";

            if (!subnauticaPath.IsEmpty())
            {
                gameResourcesPath = Path.Combine(subnauticaPath.Get(), "Subnautica_Data", "resources.assets");
            }

            if (File.Exists(gameResourcesPath))
            {
                return gameResourcesPath;
            }
            else if (File.Exists(Path.Combine("..", "resources.assets")))   //  SubServer => Subnautica/Subnautica_Data/SubServer
            {
                return Path.GetFullPath(Path.Combine("..", "resources.assets"));
            }
            else if (File.Exists(Path.Combine("..", "Subnautica_Data", "resources.assets")))   //  SubServer => Subnautica/SubServer
            {
                return Path.GetFullPath(Path.Combine("..", "Subnautica_Data", "resources.assets"));
            }
            else if (File.Exists("resources.assets"))   //  SubServer/* => Subnautica/Subnautica_Data/
            {
                return Path.GetFullPath("resources.assets");
            }
            else
            {
                throw new FileNotFoundException("Make sure resources.assets is in current or parent directory and readable.");
            }
        }
    }
}
