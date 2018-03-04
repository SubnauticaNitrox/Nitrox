using System;
using System.Collections.Generic;
using System.IO;
using AssetsTools.NET;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxServer.Serialization;
using UWE;
using static LootDistributionData;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public class BatchEntitySpawner : IEntitySpawner
    {
        private readonly HashSet<Int3> parsedBatches = new HashSet<Int3>();
        private readonly Dictionary<string, WorldEntityInfo> worldEntitiesByClassId;
        private readonly LootDistributionData lootDistributionData;
        private readonly BatchCellsParser batchCellsParser;
        private readonly Random random = new Random();

        private const uint TEXT_CLASS_ID = 0x31;
        private const uint MONOBEHAVIOUR_CLASS_ID = 0x72;

        public BatchEntitySpawner()
        {
            string lootDistributionString;
            if (GetDataFiles(out lootDistributionString, out worldEntitiesByClassId))
            {
                // TODO: If data files can't be loaded the code will crash due to NRE's.
                batchCellsParser = new BatchCellsParser();

                LootDistributionsParser lootDistributionsParser = new LootDistributionsParser();
                lootDistributionData = lootDistributionsParser.GetLootDistributionData(lootDistributionString);
            }
        }
        
        public List<Entity> LoadUnspawnedEntities(Int3 batchId)
        {
            lock (parsedBatches)
            {
                if (parsedBatches.Contains(batchId))
                {
                    return new List<Entity>();
                }
                parsedBatches.Add(batchId);
            }

            Log.Debug("Batch {0} not parsed yet; parsing...", batchId);

            List<Entity> entities = new List<Entity>();

            foreach (EntitySpawnPoint esp in batchCellsParser.ParseBatchData(batchId))
            {
                entities.AddRange(SpawnEntities(esp));
            }

            return entities;
        }

        private IEnumerable<Entity> SpawnEntities(EntitySpawnPoint entitySpawnPoint)
        {
            DstData dstData;
            if (!lootDistributionData.GetBiomeLoot(entitySpawnPoint.BiomeType, out dstData))
            {
                yield break;
            }

            float rollingProbabilityDensity = 0;

            PrefabData selectedPrefab = null;

            foreach (PrefabData prefab in dstData.prefabs)
            {
                float probabilityDensity = prefab.probability / entitySpawnPoint.Density;
                rollingProbabilityDensity += probabilityDensity;
            }

            double randomNumber = random.NextDouble();
            double rollingProbability = 0;

            if (rollingProbabilityDensity > 0)
            {
                if (rollingProbabilityDensity > 1f)
                {
                    randomNumber *= rollingProbabilityDensity;
                }

                foreach (PrefabData prefab in dstData.prefabs)
                {
                    float probabilityDensity = prefab.probability / entitySpawnPoint.Density;
                    rollingProbability += probabilityDensity;
                    // This is pretty hacky, it rerolls until its hits a prefab of a correct type
                    // What should happen is that we check wei first, then grab data from there
                    bool isValidSpawn = IsValidSpawnType(prefab.classId, entitySpawnPoint.CanSpawnCreature);
                    if (rollingProbability >= randomNumber && isValidSpawn)
                    {
                        selectedPrefab = prefab;
                        break;
                    }
                }
            }

            WorldEntityInfo worldEntityInfo;
            if (!ReferenceEquals(selectedPrefab, null) && worldEntitiesByClassId.TryGetValue(selectedPrefab.classId, out worldEntityInfo))
            {
                for (int i = 0; i < selectedPrefab.count; i++)
                {
                    Entity spawnedEntity = new Entity(entitySpawnPoint.Position,
                                                      entitySpawnPoint.Rotation,
                                                      worldEntityInfo.techType,
                                                      (int)worldEntityInfo.cellLevel,
                                                      selectedPrefab.classId);
                    yield return spawnedEntity;
                    
                    if (TryAssigningChildEntity(spawnedEntity))
                    {
                        yield return spawnedEntity.ChildEntity.Get();
                    }
                }
            }
        }

        private bool IsValidSpawnType(string id, bool creatureSpawn)
        {
            WorldEntityInfo worldEntityInfo;
            if (worldEntitiesByClassId.TryGetValue(id, out worldEntityInfo))
            {
                return (creatureSpawn == (worldEntityInfo.slotType == EntitySlot.Type.Creature));
            }

            return false;
        }

        private bool TryAssigningChildEntity(Entity parentEntity)
        {
            Entity childEntity = null;

            if (parentEntity.TechType == TechType.CrashHome)
            {
                childEntity = new Entity(parentEntity.Position, parentEntity.Rotation, TechType.Crash, parentEntity.Level, parentEntity.ClassId);
            }

            parentEntity.ChildEntity = Optional<Entity>.OfNullable(childEntity);

            return parentEntity.ChildEntity.IsPresent();
        }

        private bool GetDataFiles(out string lootDistributions, out Dictionary<string, WorldEntityInfo> worldEntityData)
        {
            lootDistributions = "";
            worldEntityData = new Dictionary<string, WorldEntityInfo>();
            string resourcesPath = "";
            Optional<string> steamPath = SteamFinder.FindSteamGamePath(264710, "Subnautica");
            string gameResourcesPath = "";
            if (!steamPath.IsEmpty())
            {
                gameResourcesPath = Path.Combine(steamPath.Get(), "Subnautica_Data/resources.assets");
            }

            if (File.Exists(gameResourcesPath))
            {
                resourcesPath = gameResourcesPath;
            }
            else if (File.Exists("../resources.assets"))
            {
                resourcesPath = Path.GetFullPath("../resources.assets");
            }
            else if (File.Exists("resources.assets"))
            {
                resourcesPath = Path.GetFullPath("resources.assets");
            }
            else
            {
                throw new FileNotFoundException("Make sure resources.assets is in current or parent directory and readable.");
            }

            using (FileStream resStream = new FileStream(resourcesPath, FileMode.Open))
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
                            lootDistributions = resourcesFile.reader.ReadCountStringInt32().Replace("\\n", "");
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
                                worldEntityData.Add(wei.classId, wei);
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
