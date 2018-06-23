﻿using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxServer.Serialization;
using UWE;
using static LootDistributionData;
using NitroxServer.GameLogic.Entities.Spawning.EntityBootstrappers;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public class BatchEntitySpawner : IEntitySpawner
    {
        public HashSet<Int3> ParsedBatches { get; }

        private readonly Dictionary<string, WorldEntityInfo> worldEntitiesByClassId;
        private readonly LootDistributionData lootDistributionData;
        private readonly BatchCellsParser batchCellsParser;
        private readonly Random random = new Random();
        private readonly Dictionary<TechType, IEntityBootstrapper> customBootstrappersByTechType = new Dictionary<TechType, IEntityBootstrapper>();

        public BatchEntitySpawner(ResourceAssets resourceAssets, HashSet<Int3> loadedPreviousParsed)
        {
            ParsedBatches = loadedPreviousParsed;
            worldEntitiesByClassId = resourceAssets.WorldEntitiesByClassId;
            batchCellsParser = new BatchCellsParser();

            LootDistributionsParser lootDistributionsParser = new LootDistributionsParser();
            lootDistributionData = lootDistributionsParser.GetLootDistributionData(resourceAssets.LootDistributionsJson);

            customBootstrappersByTechType[TechType.CrashHome] = new CrashFishBootstrapper();
            customBootstrappersByTechType[TechType.Reefback] = new ReefbackBootstrapper();
        }

        public List<Entity> LoadUnspawnedEntities(Int3 batchId)
        {
            lock (ParsedBatches)
            {
                if (ParsedBatches.Contains(batchId))
                {
                    return new List<Entity>();
                }
                ParsedBatches.Add(batchId);
            }

            Log.Debug("Batch {0} not parsed yet; parsing...", batchId);

            List<Entity> entities = new List<Entity>();

            foreach (EntitySpawnPoint esp in batchCellsParser.ParseBatchData(batchId))
            {
                if (esp.Density > 0)
                {
                    DstData dstData;
                    if (lootDistributionData.GetBiomeLoot(esp.BiomeType, out dstData))
                    {
                        entities.AddRange(SpawnEntitiesUsingRandomDistribution(esp, dstData));
                    }
                    else if(esp.ClassId != null)
                    {
                        entities.AddRange(SpawnEntitiesStaticly(esp));
                    }
                }
            }

            return entities;
        }

        private IEnumerable<Entity> SpawnEntitiesUsingRandomDistribution(EntitySpawnPoint entitySpawnPoint, DstData dstData)
        {
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
                if(prefab.probability == 0)
                {
                    return false;
                }

                float probabilityDensity = prefab.probability / entitySpawnPoint.Density;
                rollingProbability += probabilityDensity;
                bool isValidSpawn = IsValidSpawnType(prefab.classId, entitySpawnPoint.CanSpawnCreature);
                return rollingProbability >= randomNumber && isValidSpawn;
            });

            WorldEntityInfo worldEntityInfo;
            if (!ReferenceEquals(selectedPrefab, null) && worldEntitiesByClassId.TryGetValue(selectedPrefab.classId, out worldEntityInfo))
            {
                for (int i = 0; i < selectedPrefab.count; i++)
                {
                    IEnumerable<Entity> entities = CreateEntityWithChildren(entitySpawnPoint, 
                                                                            worldEntityInfo.techType, 
                                                                            worldEntityInfo.cellLevel, 
                                                                            selectedPrefab.classId);
                    foreach (Entity entity in entities)
                    {
                        yield return entity;
                    }
                }
            }
        }

        private IEnumerable<Entity> SpawnEntitiesStaticly(EntitySpawnPoint entitySpawnPoint)
        {
            WorldEntityInfo worldEntityInfo;
            if (worldEntitiesByClassId.TryGetValue(entitySpawnPoint.ClassId, out worldEntityInfo))
            {
                IEnumerable<Entity> entities = CreateEntityWithChildren(entitySpawnPoint, 
                                                                        worldEntityInfo.techType, 
                                                                        worldEntityInfo.cellLevel, 
                                                                        entitySpawnPoint.ClassId);
                foreach (Entity entity in entities)
                {
                    yield return entity;
                }
            }
        }

        private IEnumerable<Entity> CreateEntityWithChildren(EntitySpawnPoint entitySpawnPoint, TechType techType, LargeWorldEntity.CellLevel cellLevel, string classId)
        {
            Entity spawnedEntity = new Entity(entitySpawnPoint.Position,
                                              entitySpawnPoint.Rotation,
                                              techType,
                                              (int)cellLevel,
                                              classId);
            yield return spawnedEntity;

            IEntityBootstrapper bootstrapper;
            if (customBootstrappersByTechType.TryGetValue(spawnedEntity.TechType, out bootstrapper))
            {
                bootstrapper.Prepare(spawnedEntity);

                foreach(Entity childEntity in spawnedEntity.ChildEntities)
                {
                    yield return childEntity;
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
    }
}
