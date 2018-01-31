using AssetsTools.NET;
using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.GameLogic;
using NitroxModel.Logger;
using NitroxServer.GameLogic.Spawning;
using NitroxServer.Serialization;
using UWE;
using static LootDistributionData;
using System.IO;

namespace NitroxServer.GameLogic
{
    public class EntitySpawner
    {
        private Dictionary<AbsoluteEntityCell, List<Entity>> entitiesByAbsoluteCell;

        private readonly Dictionary<string, WorldEntityInfo> worldEntitiesByClassId;
        private readonly IEnumerable<EntitySpawnPoint> entitySpawnPoints;
        private readonly LootDistributionData lootDistributionData;

        public EntitySpawner()
        {
            string lootDistributionString = "";
            if (GetDataFiles(out lootDistributionString, out worldEntitiesByClassId))
            {
                BatchCellsParser BatchCellsParser = new BatchCellsParser();
                entitySpawnPoints = BatchCellsParser.GetEntitySpawnPoints();

                LootDistributionsParser lootDistributionsParser = new LootDistributionsParser();
                lootDistributionData = lootDistributionsParser.GetLootDistributionData(lootDistributionString);

                SpawnEntities();
            }
        }

        public Dictionary<AbsoluteEntityCell, List<Entity>> GetEntitiesByAbsoluteCell()
        {
            return entitiesByAbsoluteCell;
        }

        private void SpawnEntities()
        {
            Log.Info("Spawning entities...");
            entitiesByAbsoluteCell = new Dictionary<AbsoluteEntityCell, List<Entity>>();
            Random random = new Random();

            foreach (EntitySpawnPoint entitySpawnPoint in entitySpawnPoints)
            {
                DstData dstData;
                if (!lootDistributionData.GetBiomeLoot(entitySpawnPoint.BiomeType, out dstData))
                {
                    continue;
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
                        //This is pretty hacky, it rerolls until its hits a prefab of a correct type
                        //What should happen is that we check wei first, then grab data from there
                        bool isValidSpawn = IsValidSpawnType(prefab.classId, entitySpawnPoint.CanSpawnCreature);
                        if (rollingProbability >= randomNumber && isValidSpawn)
                        {
                            selectedPrefab = prefab;
                            break;
                        }
                    }
                }

                if (!ReferenceEquals(selectedPrefab, null) && worldEntitiesByClassId.ContainsKey(selectedPrefab.classId))
                {
                    WorldEntityInfo worldEntityInfo = worldEntitiesByClassId[selectedPrefab.classId];

                    for (int i = 0; i < selectedPrefab.count; i++)
                    {
                        Entity spawnedEntity = new Entity(entitySpawnPoint.Position,
                                                          entitySpawnPoint.Rotation,
                                                          worldEntityInfo.techType,
                                                          Guid.NewGuid().ToString(),
                                                          (int)worldEntityInfo.cellLevel);

                        AbsoluteEntityCell absoluteCellId = new AbsoluteEntityCell(entitySpawnPoint.BatchId, entitySpawnPoint.CellId);

                        if (!entitiesByAbsoluteCell.ContainsKey(absoluteCellId))
                        {
                            entitiesByAbsoluteCell[absoluteCellId] = new List<Entity>();
                        }

                        entitiesByAbsoluteCell[absoluteCellId].Add(spawnedEntity);
                    }
                }
            }
        }

        private bool IsValidSpawnType(string id, bool creatureSpawn)
        {
            if (worldEntitiesByClassId.ContainsKey(id))
            {
                WorldEntityInfo worldEntityInfo = worldEntitiesByClassId[id];
                if (creatureSpawn && worldEntityInfo.slotType == EntitySlot.Type.Creature)
                {
                    return true;
                } else if (!creatureSpawn && worldEntityInfo.slotType != EntitySlot.Type.Creature)
                {
                    return true;
                }
            }
            return false;
        }

        private bool GetDataFiles(out string lootDistributions, out Dictionary<string, WorldEntityInfo> worldEntityData)
        {
            lootDistributions = "";
            worldEntityData = new Dictionary<string, WorldEntityInfo>();
            string resourcesPath = "";
            AssetsFile resourcesFile;
            if (File.Exists("../resources.assets"))
            {
                resourcesPath = Path.GetFullPath("../resources.assets");
            }
            else if (File.Exists("resources.assets"))
            {
                resourcesPath = Path.GetFullPath("resources.assets");
            }
            else
            {
                Log.Error("Could not find resources.assets in parent or current directory.", new FileNotFoundException());
                return false;
            }

            using (FileStream resStream = new FileStream(resourcesPath, FileMode.Open))
            {
                resourcesFile = new AssetsFile(new AssetsFileReader(resStream));
                AssetsFileTable resourcesFileTable = new AssetsFileTable(resourcesFile);
                foreach (AssetFileInfoEx afi in resourcesFileTable.pAssetFileInfo)
                {
                    //ids may change in the future!
                    if (afi.curFileType == 0x31) //TextAsset
                    {
                        resourcesFile.reader.Position = afi.absoluteFilePos;
                        string assetName = resourcesFile.reader.ReadCountStringInt32();
                        if (assetName == "EntityDistributions")
                        {
                            resourcesFile.reader.Align();
                            lootDistributions = resourcesFile.reader.ReadCountStringInt32().Replace("\\n", "");
                        }
                    } else if (afi.curFileType == 0x72) //MonoBehaviour
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
