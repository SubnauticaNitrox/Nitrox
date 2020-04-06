using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.Serialization;
using NitroxServer.Serialization.Resources.Datastructures;
using UnityEngine;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public class BatchEntitySpawner : IEntitySpawner
    {
        private readonly BatchCellsParser batchCellsParser;

        private readonly Dictionary<TechType, IEntityBootstrapper> customBootstrappersByTechType;
        private readonly HashSet<Int3> emptyBatches = new HashSet<Int3>();
        private readonly Dictionary<string, List<PrefabAsset>> placeholderPrefabsByGroupClassId;
        private readonly UwePrefabFactory prefabFactory;

        private readonly UweWorldEntityFactory worldEntityFactory;

        private readonly object parsedBatchesLock = new object();
        private readonly object emptyBatchesLock = new object();
        private HashSet<Int3> parsedBatches;

        public List<Int3> SerializableParsedBatches
        {
            get
            {
                List<Int3> parsed;
                List<Int3> empty;

                lock (parsedBatchesLock)
                {
                    parsed = new List<Int3>(parsedBatches);
                }

                lock (emptyBatchesLock)
                {
                    empty = new List<Int3>(emptyBatches);
                }

                return parsed.Except(empty).ToList();
            }
            set
            {
                lock (parsedBatchesLock)
                {
                    parsedBatches = new HashSet<Int3>(value);
                }
            }
        }

        public BatchEntitySpawner(EntitySpawnPointFactory entitySpawnPointFactory, UweWorldEntityFactory worldEntityFactory, UwePrefabFactory prefabFactory, List<Int3> loadedPreviousParsed, ServerProtobufSerializer serializer,
                                  Dictionary<TechType, IEntityBootstrapper> customBootstrappersByTechType, Dictionary<string, List<PrefabAsset>> placeholderPrefabsByGroupClassId)
        {
            parsedBatches = new HashSet<Int3>(loadedPreviousParsed);
            this.worldEntityFactory = worldEntityFactory;
            this.prefabFactory = prefabFactory;
            this.customBootstrappersByTechType = customBootstrappersByTechType;
            this.placeholderPrefabsByGroupClassId = placeholderPrefabsByGroupClassId;

            batchCellsParser = new BatchCellsParser(entitySpawnPointFactory, serializer);
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

            DeterministicBatchGenerator deterministicBatchGenerator = new DeterministicBatchGenerator(batchId);
            List<EntitySpawnPoint> spawnPoints = batchCellsParser.ParseBatchData(batchId);
            List<Entity> entities = SpawnEntities(spawnPoints, deterministicBatchGenerator);

            if (entities.Count == 0)
            {
                lock (emptyBatchesLock)
                {
                    emptyBatches.Add(batchId);
                }
            }
            else
            {
                Log.Info("Spawning " + entities.Count + " entities from " + spawnPoints.Count + " spawn points in batch " + batchId);
            }

            for (int x = 0; x < entities.Count; x++) // Throws on duplicate Entities already but nice to know which ones
            {
                for (int y = 0; y < entities.Count; y++)
                {
                    if (entities[x] == entities[y] && x != y)
                    {
                        Log.Error("Duplicate Entity detected! " + entities[x]);
                    }
                }
            }

            return entities;
        }

        private IEnumerable<Entity> SpawnEntitiesUsingRandomDistribution(EntitySpawnPoint entitySpawnPoint, List<UwePrefab> prefabs, DeterministicBatchGenerator deterministicBatchGenerator, Entity parentEntity = null)
        {
            List<UwePrefab> allowedPrefabs = FilterAllowedPrefabs(prefabs, entitySpawnPoint);

            float rollingProbabilityDensity = allowedPrefabs.Sum(prefab => prefab.Probability / entitySpawnPoint.Density);

            if (rollingProbabilityDensity <= 0)
            {
                yield break;
            }

            double randomNumber = deterministicBatchGenerator.NextDouble();
            if (rollingProbabilityDensity > 1f)
            {
                randomNumber *= rollingProbabilityDensity;
            }

            double rollingProbability = 0;

            UwePrefab selectedPrefab = allowedPrefabs.FirstOrDefault(prefab =>
            {
                if (Math.Abs(prefab.Probability) < 0.0001)
                {
                    return false;
                }

                float probabilityDensity = prefab.Probability / entitySpawnPoint.Density;

                rollingProbability += probabilityDensity;

                return rollingProbability >= randomNumber;
            });

            if (selectedPrefab == null)
            {
                yield break;
            }

            Optional<UweWorldEntity> opWorldEntity = worldEntityFactory.From(selectedPrefab.ClassId);

            if (opWorldEntity.HasValue)
            {
                UweWorldEntity uweWorldEntity = opWorldEntity.Value;

                for (int i = 0; i < selectedPrefab.Count; i++)
                {
                    IEnumerable<Entity> entities = CreateEntityWithChildren(entitySpawnPoint,
                                                                            uweWorldEntity.Scale,
                                                                            uweWorldEntity.TechType,
                                                                            uweWorldEntity.CellLevel,
                                                                            selectedPrefab.ClassId,
                                                                            deterministicBatchGenerator,
                                                                            parentEntity);
                    foreach (Entity entity in entities)
                    {
                        yield return entity;
                    }
                }
            }
        }

        private List<UwePrefab> FilterAllowedPrefabs(List<UwePrefab> prefabs, EntitySpawnPoint entitySpawnPoint)
        {
            List<UwePrefab> allowedPrefabs = new List<UwePrefab>();

            foreach (UwePrefab prefab in prefabs)
            {
                if (prefab.ClassId != "None")
                {
                    Optional<UweWorldEntity> uweWorldEntity = worldEntityFactory.From(prefab.ClassId);

                    if (uweWorldEntity.HasValue && entitySpawnPoint.AllowedTypes.Contains(uweWorldEntity.Value.SlotType))
                    {
                        allowedPrefabs.Add(prefab);
                    }
                }
            }

            return allowedPrefabs;
        }

        private IEnumerable<Entity> SpawnEntitiesStaticly(EntitySpawnPoint entitySpawnPoint, DeterministicBatchGenerator deterministicBatchGenerator, Entity parentEntity = null)
        {
            Optional<UweWorldEntity> uweWorldEntity = worldEntityFactory.From(entitySpawnPoint.ClassId);

            if (uweWorldEntity.HasValue)
            {
                IEnumerable<Entity> entities = CreateEntityWithChildren(entitySpawnPoint,
                                                                        entitySpawnPoint.Scale,
                                                                        uweWorldEntity.Value.TechType,
                                                                        uweWorldEntity.Value.CellLevel,
                                                                        entitySpawnPoint.ClassId,
                                                                        deterministicBatchGenerator,
                                                                        parentEntity);
                foreach (Entity entity in entities)
                {
                    yield return entity;
                }
            }
        }

        private IEnumerable<Entity> CreateEntityWithChildren(EntitySpawnPoint entitySpawnPoint, Vector3 scale, TechType techType, int cellLevel, string classId, DeterministicBatchGenerator deterministicBatchGenerator, Entity parentEntity = null)
        {
            Entity spawnedEntity = new Entity(entitySpawnPoint.LocalPosition,
                                              entitySpawnPoint.LocalRotation,
                                              scale,
                                              techType,
                                              cellLevel,
                                              classId,
                                              true,
                                              deterministicBatchGenerator.NextId(),
                                              parentEntity);

            spawnedEntity.ChildEntities = SpawnEntities(entitySpawnPoint.Children, deterministicBatchGenerator, spawnedEntity);

            CreatePrefabPlaceholdersWithChildren(spawnedEntity, classId, deterministicBatchGenerator);

            IEntityBootstrapper bootstrapper;

            if (customBootstrappersByTechType.TryGetValue(techType, out bootstrapper))
            {
                bootstrapper.Prepare(spawnedEntity, deterministicBatchGenerator);
            }

            yield return spawnedEntity;

            if (parentEntity == null) // Ensures children are only returned at the top level
            {
                // Children are yielded as well so they can be indexed at the top level (for use by simulation 
                // ownership and various other consumers).  The parent should always be yielded before the children
                foreach (Entity childEntity in AllChildren(spawnedEntity))
                {
                    yield return childEntity;
                }
            }
        }

        private IEnumerable<Entity> AllChildren(Entity entity)
        {
            foreach (Entity child in entity.ChildEntities)
            {
                yield return child;

                if (child.ChildEntities.Count > 0)
                {
                    foreach (Entity childOfChild in AllChildren(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private List<Entity> SpawnEntities(List<EntitySpawnPoint> entitySpawnPoints, DeterministicBatchGenerator deterministicBatchGenerator, Entity parentEntity = null)
        {
            List<Entity> entities = new List<Entity>();
            foreach (EntitySpawnPoint esp in entitySpawnPoints)
            {
                if (esp.Density > 0)
                {
                    List<UwePrefab> prefabs = prefabFactory.GetPossiblePrefabs(esp.BiomeType);

                    if (prefabs.Count > 0)
                    {
                        entities.AddRange(SpawnEntitiesUsingRandomDistribution(esp, prefabs, deterministicBatchGenerator, parentEntity));
                    }
                    else if (esp.ClassId != null)
                    {
                        entities.AddRange(SpawnEntitiesStaticly(esp, deterministicBatchGenerator, parentEntity));
                    }
                }
            }
            return entities;
        }

        private void CreatePrefabPlaceholdersWithChildren(Entity entity, string classId, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            List<PrefabAsset> prefabs;

            // Check to see if this entity is a PrefabPlaceholderGroup.  If it is, 
            // we want to add the children that would be spawned here.  This is 
            // surpressed on the client so we don't get virtual entities that the
            // server doesn't know about.
            if (placeholderPrefabsByGroupClassId.TryGetValue(classId, out prefabs))
            {
                foreach (PrefabAsset prefab in prefabs)
                {
                    TransformAsset transform = prefab.TransformAsset;

                    Optional<UweWorldEntity> opWorldEntity = worldEntityFactory.From(prefab.ClassId);

                    if (!opWorldEntity.HasValue)
                    {
                        Log.Debug("Unexpected Empty WorldEntity! " + prefab.ClassId);
                        continue;
                    }

                    UweWorldEntity worldEntity = opWorldEntity.Value;
                    Entity prefabEntity = new Entity(transform.LocalPosition,
                                                     transform.LocalRotation,
                                                     transform.LocalScale,
                                                     worldEntity.TechType,
                                                     worldEntity.CellLevel,
                                                     prefab.ClassId,
                                                     true,
                                                     deterministicBatchGenerator.NextId(),
                                                     entity);

                    if (prefab.EntitySlot.HasValue)
                    {
                        Entity possibleEntity = SpawnEntitySlotEntities(prefab.EntitySlot.Value, transform, deterministicBatchGenerator, entity);
                        if (possibleEntity != null)
                        {
                            entity.ChildEntities.Add(possibleEntity);
                        }
                    }

                    CreatePrefabPlaceholdersWithChildren(prefabEntity, prefabEntity.ClassId, deterministicBatchGenerator);
                    entity.ChildEntities.Add(prefabEntity);
                }
            }
        }

        private Entity SpawnEntitySlotEntities(NitroxEntitySlot entitySlot, TransformAsset transform, DeterministicBatchGenerator deterministicBatchGenerator, Entity parentEntity)
        {
            List<UwePrefab> prefabs = prefabFactory.GetPossiblePrefabs(entitySlot.BiomeType);
            List<Entity> entities = new List<Entity>();

            if (prefabs.Count > 0)
            {
                EntitySpawnPoint entitySpawnPoint = new EntitySpawnPoint(parentEntity.AbsoluteEntityCell, transform.LocalPosition, transform.LocalRotation, entitySlot.AllowedTypes.ToList(), 1f, entitySlot.BiomeType);
                entities.AddRange(SpawnEntitiesUsingRandomDistribution(entitySpawnPoint, prefabs, deterministicBatchGenerator, parentEntity));
            }

            return entities.FirstOrDefault();
        }
    }
}
