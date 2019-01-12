using System;
using System.Collections.Generic;
using System.IO;
using AssetsTools.NET;
using NitroxModel.DataStructures.Util;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UWE;

namespace NitroxServer.Serialization
{
    public static class ResourceAssetsParser
    {
        private const uint TEXT_CLASS_ID = 0x31;
        private const uint MONOBEHAVIOUR_CLASS_ID = 0x72;

        public static ResourceAssets Parse()
        {
            ResourceAssets resourceAssets = new ResourceAssets();

            using (FileStream resStream = new FileStream(FindPath(), FileMode.Open))
            using (AssetsFileReader resReader = new AssetsFileReader(resStream))
            {
                AssetsFile resourcesFile = new AssetsFile(resReader);
                AssetsFileTable resourcesFileTable = new AssetsFileTable(resourcesFile);
                foreach (AssetFileInfoEx afi in resourcesFileTable.pAssetFileInfo)
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
                        resourcesFile.reader.Position += 28;
                        string assetName = resourcesFile.reader.ReadCountStringInt32();
                        if (assetName == "WorldEntityData")
                        {
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
                        }
                    }
                }
            }

            Validate.IsTrue(resourceAssets.WorldEntitiesByClassId.Count > 0);
            Validate.IsTrue(resourceAssets.LootDistributionsJson != "");

            return resourceAssets;
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
            else if (File.Exists(Path.Combine("..", "resources.assets")))
            {
                return Path.GetFullPath(Path.Combine("..", "resources.assets"));
            }
            else if (File.Exists("resources.assets"))
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
