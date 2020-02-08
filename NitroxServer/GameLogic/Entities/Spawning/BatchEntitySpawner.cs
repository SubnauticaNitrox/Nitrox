using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxServer.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxServer.GameLogic.Entities.EntityBootstrappers;
using NitroxServer.Serialization.Resources.Datastructures;
using UnityEngine;
using System;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public class BatchEntitySpawner : IEntitySpawner
    {
        public List<Int3> SerializableParsedBatches
        {
            get
            {
                List<Int3> parsed;
                List<Int3> empty;

                lock (parsedBatches)
                {
                    parsed = new List<Int3>(parsedBatches);
                }

                lock (emptyBatches)
                {
                    empty =  new List<Int3>(emptyBatches);
                }

                return parsed.Except(empty).ToList();
            }
            set { parsedBatches = new HashSet<Int3>(value); }
        }        

        private HashSet<Int3> parsedBatches = new HashSet<Int3>();
        private HashSet<Int3> emptyBatches = new HashSet<Int3>();

        private readonly UweWorldEntityFactory worldEntityFactory;
        private readonly UwePrefabFactory prefabFactory;
        private readonly BatchCellsParser batchCellsParser;

        private readonly Dictionary<TechType, IEntityBootstrapper> customBootstrappersByTechType;
        private readonly Dictionary<string, List<PrefabAsset>> placeholderPrefabsByGroupClassId;

        public BatchEntitySpawner(EntitySpawnPointFactory entitySpawnPointFactory, UweWorldEntityFactory worldEntityFactory, UwePrefabFactory prefabFactory, List<Int3> loadedPreviousParsed, ServerProtobufSerializer serializer, Dictionary<TechType, IEntityBootstrapper> customBootstrappersByTechType, Dictionary<string, List<PrefabAsset>> placeholderPrefabsByGroupClassId)
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

            List<Entity> entities = new List<Entity>();
            List<EntitySpawnPoint> spawnPoints = batchCellsParser.ParseBatchData(batchId);

            entities = SpawnEntities(spawnPoints, deterministicBatchGenerator);

            if(entities.Count == 0)
            {
                lock(emptyBatches)
                {
                    emptyBatches.Add(batchId);
                }
            }
            else
            {
                Log.Info("Spawning " + entities.Count + " entities from " + spawnPoints.Count + " spawn points in batch " + batchId);
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
                if(prefab.Probability == 0)
                {
                    return false;
                }

                float probabilityDensity = prefab.Probability / entitySpawnPoint.Density;

                rollingProbability += probabilityDensity;

                return rollingProbability >= randomNumber;
            });

            if(selectedPrefab == null)
            {
                yield break;
            }

            Optional<UweWorldEntity> opWorldEntity = worldEntityFactory.From(selectedPrefab.ClassId);
            
            if (opWorldEntity.IsPresent())
            {
                UweWorldEntity uweWorldEntity = opWorldEntity.Get();

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

            foreach(UwePrefab prefab in prefabs)
            {
                if (prefab.ClassId != "None")
                {
                    Optional<UweWorldEntity> uweWorldEntity = worldEntityFactory.From(prefab.ClassId);

                    if (uweWorldEntity.IsPresent() && entitySpawnPoint.AllowedTypes.Contains(uweWorldEntity.Get().SlotType))
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

            if (uweWorldEntity.IsPresent())
            {
                IEnumerable<Entity> entities = CreateEntityWithChildren(entitySpawnPoint,
                                                                        entitySpawnPoint.Scale,
                                                                        uweWorldEntity.Get().TechType,
                                                                        uweWorldEntity.Get().CellLevel, 
                                                                        entitySpawnPoint.ClassId,
                                                                        deterministicBatchGenerator,
                                                                        parentEntity);
                foreach (Entity entity in entities)
                {
                    yield return entity;
                }
            }
        }

        private IEnumerable<Entity> CreateEntityWithChildren(EntitySpawnPoint entitySpawnPoint, UnityEngine.Vector3 scale, TechType techType, int cellLevel, string classId, DeterministicBatchGenerator deterministicBatchGenerator, Entity parentEntity = null)
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

            AssignPlaceholderEntitiesIfRequired(spawnedEntity, classId, deterministicBatchGenerator);

            IEntityBootstrapper bootstrapper;

            if (customBootstrappersByTechType.TryGetValue(techType, out bootstrapper))
            {
                bootstrapper.Prepare(spawnedEntity, deterministicBatchGenerator);
            }

            yield return spawnedEntity;

            // Children are yielded as well so they can be indexed at the top level (for use by simulation 
            // ownership and various other consumers).  The parent should always be yielded before the children
            foreach (Entity childEntity in spawnedEntity.ChildEntities)
            {
                yield return childEntity;
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

        private void AssignPlaceholderEntitiesIfRequired(Entity entity, string classId, DeterministicBatchGenerator deterministicBatchGenerator)
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

                    if (opWorldEntity.IsEmpty())
                    {
                        Log.Debug("Unexpected Empty WorldEntity! " + prefab.ClassId);
                        continue;
                    }

                    UweWorldEntity worldEntity = opWorldEntity.Get();

                    Entity prefabEntity = new Entity(transform.Position,
                                            transform.Rotation,
                                            transform.Scale,
                                            worldEntity.TechType,
                                            worldEntity.CellLevel,
                                            prefab.ClassId,
                                            true,
                                            deterministicBatchGenerator.NextId(),
                                            entity);

                    entity.ChildEntities.Add(prefabEntity);
                    System.Diagnostics.Debug.WriteLine("Parent: " + entity + ", \nChild" + prefabEntity);
                }
            }
        }
    }
}
