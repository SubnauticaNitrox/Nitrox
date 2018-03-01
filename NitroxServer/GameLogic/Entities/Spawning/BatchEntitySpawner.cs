using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
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

        public BatchEntitySpawner(ResourceAssets resourceAssets)
        {
            worldEntitiesByClassId = resourceAssets.WorldEntitiesByClassId;
            batchCellsParser = new BatchCellsParser();

            LootDistributionsParser lootDistributionsParser = new LootDistributionsParser();
            lootDistributionData = lootDistributionsParser.GetLootDistributionData(resourceAssets.LootDistributionsJson);
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

            float rollingProbabilityDensity = dstData.prefabs.Sum(prefab => prefab.probability / entitySpawnPoint.Density);

            if (rollingProbabilityDensity <= 0)
            {
                yield break;
            }

            double randomNumber = random.NextDouble();
            if (rollingProbabilityDensity > 1f)
            {
                randomNumber *= rollingProbabilityDensity;
            }

            double rollingProbability = 0;
            PrefabData selectedPrefab = dstData.prefabs.FirstOrDefault(prefab =>
            {
                float probabilityDensity = prefab.probability / entitySpawnPoint.Density;
                rollingProbability += probabilityDensity;
                // This is pretty hacky, it rerolls until its hits a prefab of a correct type
                // What should happen is that we check wei first, then grab data from there
                bool isValidSpawn = IsValidSpawnType(prefab.classId, entitySpawnPoint.CanSpawnCreature);
                return rollingProbability >= randomNumber && isValidSpawn;
            });

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
    }
}
