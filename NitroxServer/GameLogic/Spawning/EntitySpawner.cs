using NitroxModel.GameLogic;
using NitroxModel.Logger;
using NitroxServer.GameLogic.Spawning;
using NitroxServer.Serialization;
using System;
using System.Collections.Generic;
using UWE;
using static LootDistributionData;

namespace NitroxServer.GameLogic
{
    public class EntitySpawner
    {
        private Dictionary<Int3, List<SpawnedEntity>> entitiesByBatchId;

        private Dictionary<String, WorldEntityInfo> worldEntitiesByClassId;
        private Dictionary<Int3, List<EntitySpawnPoint>> entitySpawnPointByBatchId;
        private LootDistributionData lootDistributionData;

        public EntitySpawner()
        {
            WorldEntityDataParser worldEntityDataParser = new WorldEntityDataParser();
            worldEntitiesByClassId = worldEntityDataParser.GetWorldEntitiesByClassId();

            BatchCellsParser BatchCellsParser = new BatchCellsParser();
            entitySpawnPointByBatchId = BatchCellsParser.GetEntitySpawnPointsByBatchId();

            LootDistributionsParser lootDistributionsParser = new LootDistributionsParser();
            lootDistributionData = lootDistributionsParser.GetLootDistributionData();

            SpawnEntities();
        }

        public List<SpawnedEntity> GetEntitiesByBatchId(Int3 batchId)
        {
            return entitiesByBatchId[batchId];
        }

        private void SpawnEntities()
        {
            Log.Info("Spawning entities...");
            entitiesByBatchId = new Dictionary<Int3, List<SpawnedEntity>>();
            Random random = new Random();

            foreach (var entitySpawnPointsWithBatchId in entitySpawnPointByBatchId)
            {
                Int3 batchId = entitySpawnPointsWithBatchId.Key;
                List<EntitySpawnPoint> entitySpawnPoints = entitySpawnPointsWithBatchId.Value;

                entitiesByBatchId[batchId] = new List<SpawnedEntity>();
                
                foreach (EntitySpawnPoint spawnPoint in entitySpawnPoints)
                {
                    LootDistributionData.DstData dstData;
                    if(!lootDistributionData.GetBiomeLoot(spawnPoint.BiomeType, out dstData))
                    {
                        continue;
                    }

                    float rollingProbabilityDensity = 0;

                    PrefabData selectedPrefab = null;

                    foreach (var prefab in dstData.prefabs)
                    {
                        float probabilityDensity = prefab.probability / spawnPoint.Density;
                        rollingProbabilityDensity += probabilityDensity;
                    }

                    if (rollingProbabilityDensity > 0)
                    {
                        double randomNumber = random.NextDouble();
                        double rollingProbability = 0;

                        if (rollingProbabilityDensity > 1f)
                        {
                            randomNumber *= rollingProbabilityDensity;
                        }

                        foreach (var prefab in dstData.prefabs)
                        {
                            float probabilityDensity = prefab.probability / spawnPoint.Density;
                            rollingProbability += probabilityDensity;
                            if (rollingProbability >= randomNumber)
                            {
                                selectedPrefab = prefab;
                                break;
                            }
                        }
                    }

                    if (!ReferenceEquals(selectedPrefab, null) && worldEntitiesByClassId.ContainsKey(selectedPrefab.classId))
                    {
                        WorldEntityInfo worldEntityInfo = worldEntitiesByClassId[selectedPrefab.classId];
                        
                        for(int i = 0; i < selectedPrefab.count; i++)
                        {
                            SpawnedEntity spawnedEntity = new SpawnedEntity(spawnPoint.Position,
                                                                            worldEntityInfo.techType,
                                                                            Guid.NewGuid().ToString());
                            entitiesByBatchId[batchId].Add(spawnedEntity);
                        }                        
                    }
                }
            }
        }
    }
}
