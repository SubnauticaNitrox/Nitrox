using NitroxModel.Logger;
using NitroxServer.GameLogic.Spawning;
using NitroxServer.Serialization;
using NitroxServer.UnityStubs;
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
        private Dictionary<Int3, List<GameObject>> gameObjectsByBatchId;
        private LootDistributionData lootDistributionData;

        public EntitySpawner()
        {
            WorldEntityDataParser worldEntityDataParser = new WorldEntityDataParser();
            worldEntitiesByClassId = worldEntityDataParser.GetWorldEntitiesByClassId();

            BatchCellsParser BatchCellsParser = new BatchCellsParser();
            gameObjectsByBatchId = BatchCellsParser.GetGameObjectsByBatchId();

            LootDistributionsParser lootDistributionsParser = new LootDistributionsParser();
            LootDistributionData lootDistributionData = lootDistributionsParser.GetLootDistributionData();

            SpawnEntities();
        }

        private void SpawnEntities()
        {
            Log.Info("Spawning entities...");
            entitiesByBatchId = new Dictionary<Int3, List<SpawnedEntity>>();
            Random random = new Random();

            foreach (var gameObjectsWithBatchId in gameObjectsByBatchId)
            {
                Int3 batchId = gameObjectsWithBatchId.Key;
                List<GameObject> gameObjects = gameObjectsWithBatchId.Value;

                entitiesByBatchId[batchId] = new List<SpawnedEntity>();

                foreach (GameObject gameObject in gameObjects)
                {
                    EntitySlot entitySlot = gameObject.GetComponent<EntitySlot>();

                    if (!object.ReferenceEquals(entitySlot, null))
                    {
                        LootDistributionData.DstData dstData;
                        if(!lootDistributionData.GetBiomeLoot(entitySlot.biomeType, out dstData))
                        {
                            continue;
                        }

                        float rollingProbability = 0;
                        double randomNumber = random.NextDouble();

                        PrefabData selectedPrefab = null;

                        foreach (var prefab in dstData.prefabs)
                        {
                            float num1 = prefab.probability / entitySlot.density;
                            rollingProbability += num1;

                            if (rollingProbability >= randomNumber)
                            {
                                selectedPrefab = prefab;
                            }
                        }

                        if (!ReferenceEquals(selectedPrefab, null) && worldEntitiesByClassId.ContainsKey(selectedPrefab.classId))
                        {
                            WorldEntityInfo worldEntityInfo = worldEntitiesByClassId[selectedPrefab.classId];
                            SpawnedEntity spawnedEntity = new SpawnedEntity(gameObject.GetComponent<Transform>().Position,
                                                                            worldEntityInfo.techType,
                                                                            Guid.NewGuid().ToString(),
                                                                            entitySlot.IsCreatureSlot());
                            entitiesByBatchId[batchId].Add(spawnedEntity);
                        }
                    }
                }
            }
        }
    }
}
