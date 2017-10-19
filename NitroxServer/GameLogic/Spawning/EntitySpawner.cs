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

                    float rollingProbability = 0;
                    double randomNumber = random.NextDouble();

                    PrefabData selectedPrefab = null;

                    foreach (var prefab in dstData.prefabs)
                    {
                        float num1 = prefab.probability / spawnPoint.Density;
                        rollingProbability += num1;

                        if (rollingProbability >= randomNumber)
                        {
                            selectedPrefab = prefab;
                        }
                    }

                    if (!ReferenceEquals(selectedPrefab, null) && worldEntitiesByClassId.ContainsKey(selectedPrefab.classId))
                    {
                        WorldEntityInfo worldEntityInfo = worldEntitiesByClassId[selectedPrefab.classId];

                        if (worldEntityInfo.techType != TechType.None) //TODO: we should research why they have tech type nones in the loot distribution.
                        {
                            SpawnedEntity spawnedEntity = new SpawnedEntity(spawnPoint.Position,
                                                                            worldEntityInfo.techType,
                                                                            Guid.NewGuid().ToString(),
                                                                            spawnPoint.CanSpawnCreature);
                            entitiesByBatchId[batchId].Add(spawnedEntity);
                        }
                    }                    
                }
            }
        }
    }
}
