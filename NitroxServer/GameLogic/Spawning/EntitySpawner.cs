using NitroxModel.DataStructures.Util;
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
        private Dictionary<Int3, List<SpawnedEntity>> entitiesByAbsoluteCellId;

        private Dictionary<String, WorldEntityInfo> worldEntitiesByClassId;
        private List<EntitySpawnPoint> entitySpawnPoints;
        private LootDistributionData lootDistributionData;

        public EntitySpawner()
        {
            WorldEntityDataParser worldEntityDataParser = new WorldEntityDataParser();
            worldEntitiesByClassId = worldEntityDataParser.GetWorldEntitiesByClassId();

            BatchCellsParser BatchCellsParser = new BatchCellsParser();
            entitySpawnPoints = BatchCellsParser.GetEntitySpawnPoints();

            LootDistributionsParser lootDistributionsParser = new LootDistributionsParser();
            lootDistributionData = lootDistributionsParser.GetLootDistributionData();

            SpawnEntities();
        }

        public List<SpawnedEntity> GetEntitiesByAbsoluteCellId(Int3 absoluteCellId)
        {
            List<SpawnedEntity> entities;

            if (entitiesByAbsoluteCellId.TryGetValue(absoluteCellId, out entities))
            {
                return entities;
            }

            return new List<SpawnedEntity>();
        }

        private void SpawnEntities()
        {
            Log.Info("Spawning entities...");
            entitiesByAbsoluteCellId = new Dictionary<Int3, List<SpawnedEntity>>();
            Random random = new Random();

            foreach (EntitySpawnPoint entitySpawnPoint in entitySpawnPoints)
            {
                LootDistributionData.DstData dstData;
                if(!lootDistributionData.GetBiomeLoot(entitySpawnPoint.BiomeType, out dstData))
                {
                    continue;
                }

                float rollingProbabilityDensity = 0;

                PrefabData selectedPrefab = null;
                     
                foreach (var prefab in dstData.prefabs)
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

                    foreach (var prefab in dstData.prefabs)
                    {
                        float probabilityDensity = prefab.probability / entitySpawnPoint.Density;
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
                        SpawnedEntity spawnedEntity = new SpawnedEntity(entitySpawnPoint.Position,
                                                                        worldEntityInfo.techType,
                                                                        Guid.NewGuid().ToString(),
                                                                        Optional<String>.Empty());

                        Int3 absoluteCellId = EntityCellHelper.GetAbsoluteCellId(entitySpawnPoint.BatchId, entitySpawnPoint.CellId);

                        if(!entitiesByAbsoluteCellId.ContainsKey(absoluteCellId))
                        {
                            entitiesByAbsoluteCellId[absoluteCellId] = new List<SpawnedEntity>();
                        }

                        entitiesByAbsoluteCellId[absoluteCellId].Add(spawnedEntity);
                    }                        
                }                
            }
        }
    }
}
