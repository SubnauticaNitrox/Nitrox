using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxServer.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxServer.GameLogic.Entities.EntityBootstrappers;

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

        public BatchEntitySpawner(EntitySpawnPointFactory entitySpawnPointFactory, UweWorldEntityFactory worldEntityFactory, UwePrefabFactory prefabFactory, List<Int3> loadedPreviousParsed, ServerProtobufSerializer serializer, Dictionary<TechType, IEntityBootstrapper> customBootstrappersByTechType)
        {
            parsedBatches = new HashSet<Int3>(loadedPreviousParsed);
            this.worldEntityFactory = worldEntityFactory;
            this.prefabFactory = prefabFactory;
            this.customBootstrappersByTechType = customBootstrappersByTechType;
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
                List<UwePrefab> prefabs = prefabFactory.GetPossiblePrefabs(esp.BiomeType);

                entities.AddRange(CreateEntityCellRoot(esp, prefabs, deterministicBatchGenerator));
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
                Log.Info("Spawning " + entities.Count + " entity cells in batch " + batchId);
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
                                                                        entitySpawnPoint.LocalScale,
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
            Entity spawnedEntity = new Entity(entitySpawnPoint.LocalPosition,
                                              entitySpawnPoint.LocalRotation, 
                                              scale,
                                              techType,
                                              cellLevel,
                                              classId,
                                              true,
                                              deterministicBatchGenerator.NextGuid());

            List<Entity> prefabs = GetEntitiesFromClassId(classId, spawnedEntity, deterministicBatchGenerator);

            yield return spawnedEntity;

            IEntityBootstrapper bootstrapper;
            if (customBootstrappersByTechType.TryGetValue(techType, out bootstrapper))
            {
                bootstrapper.Prepare(spawnedEntity, deterministicBatchGenerator);

                for (int i = 0; i < spawnedEntity.ChildEntities.Count - prefabs.Count; i++)
                {
                    Entity childEntity = spawnedEntity.ChildEntities[i + prefabs.Count];
                }
            }
        }

        private IEnumerable<Entity> CreateEntityCellRoot(EntitySpawnPoint entitySpawnPoint, List<UwePrefab> prefabs, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            Entity spawnedEntity = new Entity(entitySpawnPoint.LocalPosition, // CellRoot
                                              entitySpawnPoint.LocalRotation,
                                              entitySpawnPoint.LocalScale,
                                              new TechType("None"),
                                              entitySpawnPoint.AbsoluteEntityCell.Level,
                                              entitySpawnPoint.ClassId,
                                              true,
                                              deterministicBatchGenerator.NextGuid());

            foreach (EntitySpawnPoint childSpawnPoint in entitySpawnPoint.Children)
            {
                if (childSpawnPoint.Density > 0)
                {
                    if (prefabs.Count > 0)
                    {
                        spawnedEntity.ChildEntities.AddRange(SpawnEntitiesUsingRandomDistribution(childSpawnPoint, prefabs, deterministicBatchGenerator));
                    }
                    else if (childSpawnPoint.ClassId != null)
                    {
                        spawnedEntity.ChildEntities.AddRange(SpawnEntitiesStaticly(childSpawnPoint, deterministicBatchGenerator));
                    }
                }
            }

            spawnedEntity.ChildEntities.ForEach(c => c.IsChild = true);

            yield return spawnedEntity;
        }

        private List<Entity> GetEntitiesFromClassId(string classId, Entity spawnedEntity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            List<UwePrefab> prefabClassIds = prefabFactory.GetPrefabForClassId(classId);
            List<Entity> entities = new List<Entity>();

            if (prefabClassIds.Any())
            {
                foreach (UwePrefab prefab in prefabClassIds)
                {
                    Optional<UweWorldEntity> wei = worldEntityFactory.From(prefab.ClassId);

                    prefab.Guid = deterministicBatchGenerator.NextGuid();
                    prefab.CellLevel = wei.Get().CellLevel;
                    prefab.TechType = wei.Get().TechType;

                    Entity childEntity = new Entity(prefab.LocalPosition,
                        prefab.LocalRotation,
                        prefab.LocalScale,
                        prefab.TechType,
                        prefab.CellLevel,
                        prefab.ClassId,
                        true,
                        prefab.Guid) { IsChild = true };

                    entities.Add(childEntity);

                    childEntity.ChildEntities = GetEntitiesFromClassId(prefab.ClassId, childEntity, deterministicBatchGenerator);

                    entities.AddRange(childEntity.ChildEntities);

                    spawnedEntity.ChildEntities.Add(childEntity);
                }
            }

            return entities;
        }

    }
}
