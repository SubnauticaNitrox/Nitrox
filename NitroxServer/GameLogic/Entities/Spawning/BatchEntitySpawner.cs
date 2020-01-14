﻿using System.Collections.Generic;
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

            foreach (EntitySpawnPoint esp in spawnPoints)
            {
                if (esp.Density > 0)
                {
                    List<UwePrefab> prefabs = prefabFactory.GetPossiblePrefabs(esp.BiomeType);

                    if (prefabs.Count > 0)
                    {
                        entities.AddRange(SpawnEntitiesUsingRandomDistribution(esp, prefabs, deterministicBatchGenerator));
                    }
                    else if(esp.ClassId != null)
                    {
                        entities.AddRange(SpawnEntitiesStaticly(esp, deterministicBatchGenerator));
                    }
                }
            }

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

        private IEnumerable<Entity> SpawnEntitiesUsingRandomDistribution(EntitySpawnPoint entitySpawnPoint, List<UwePrefab> prefabs, DeterministicBatchGenerator deterministicBatchGenerator)
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
                                                                            deterministicBatchGenerator);
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

        private IEnumerable<Entity> SpawnEntitiesStaticly(EntitySpawnPoint entitySpawnPoint, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            Optional<UweWorldEntity> uweWorldEntity = worldEntityFactory.From(entitySpawnPoint.ClassId);

            if (uweWorldEntity.IsPresent())
            {
                IEnumerable<Entity> entities = CreateEntityWithChildren(entitySpawnPoint,
                                                                        entitySpawnPoint.Scale,
                                                                        uweWorldEntity.Get().TechType,
                                                                        uweWorldEntity.Get().CellLevel, 
                                                                        entitySpawnPoint.ClassId,
                                                                        deterministicBatchGenerator);
                foreach (Entity entity in entities)
                {
                    yield return entity;
                }
            }
        }

        private IEnumerable<Entity> CreateEntityWithChildren(EntitySpawnPoint entitySpawnPoint, UnityEngine.Vector3 scale, TechType techType, int cellLevel, string classId, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            Entity spawnedEntity = new Entity(entitySpawnPoint.Position,
                                              entitySpawnPoint.Rotation,
                                              scale,
                                              techType,
                                              cellLevel,
                                              classId,
                                              true,
                                              deterministicBatchGenerator.NextId());
            
            
            yield return spawnedEntity;

            AssignPlaceholderEntitiesIfRequired(spawnedEntity, techType, cellLevel, classId, deterministicBatchGenerator);

            IEntityBootstrapper bootstrapper;

            if (customBootstrappersByTechType.TryGetValue(techType, out bootstrapper))
            {
                bootstrapper.Prepare(spawnedEntity, deterministicBatchGenerator);
            }

            // Children are yielded as well so they can be indexed at the top level (for use by simulation 
            // ownership and various other consumers).  The parent should always be yielded before the children
            foreach (Entity childEntity in spawnedEntity.ChildEntities)
            {
                yield return childEntity;
            }
        }

        private void AssignPlaceholderEntitiesIfRequired(Entity entity, TechType techType, int cellLevel, string classId, DeterministicBatchGenerator deterministicBatchGenerator)
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

                    Vector3 position = transform.Position + entity.Position;
                    Quaternion rotation = entity.Rotation * transform.Rotation;

                    Entity prefabEntity = new Entity(position,
                                            rotation,
                                            transform.Scale,
                                            techType,
                                            cellLevel,
                                            prefab.ClassId,
                                            true,
                                            deterministicBatchGenerator.NextId());

                    entity.ChildEntities.Add(prefabEntity);
                }
            }
        }
    }
}
