using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxServer.Helper;
using NitroxServer.Resources;
using NitroxServer.Serialization;

namespace NitroxServer.GameLogic.Entities.Spawning;

public class BatchEntitySpawner : IEntitySpawner
{
    private readonly BatchCellsParser batchCellsParser;

    private readonly Dictionary<NitroxTechType, IEntityBootstrapper> customBootstrappersByTechType;
    private readonly HashSet<NitroxInt3> emptyBatches = new();
    private readonly Dictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholderGroupsByClassId;
    private readonly UwePrefabFactory prefabFactory;

    private readonly string seed;

    private readonly UweWorldEntityFactory worldEntityFactory;

    private readonly object parsedBatchesLock = new object();
    private readonly object emptyBatchesLock = new object();
    private HashSet<NitroxInt3> parsedBatches;

    public List<NitroxInt3> SerializableParsedBatches
    {
        get
        {
            List<NitroxInt3> parsed;
            List<NitroxInt3> empty;

            lock (parsedBatchesLock)
            {
                parsed = new List<NitroxInt3>(parsedBatches);
            }

            lock (emptyBatchesLock)
            {
                empty = new List<NitroxInt3>(emptyBatches);
            }

            return parsed.Except(empty).ToList();
        }
        set
        {
            lock (parsedBatchesLock)
            {
                parsedBatches = new HashSet<NitroxInt3>(value);
            }
        }
    }

    public BatchEntitySpawner(EntitySpawnPointFactory entitySpawnPointFactory, UweWorldEntityFactory worldEntityFactory, UwePrefabFactory prefabFactory, List<NitroxInt3> loadedPreviousParsed, ServerProtoBufSerializer serializer,
                              Dictionary<NitroxTechType, IEntityBootstrapper> customBootstrappersByTechType, Dictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholderGroupsByClassId, string seed)
    {
        parsedBatches = new HashSet<NitroxInt3>(loadedPreviousParsed);
        this.worldEntityFactory = worldEntityFactory;
        this.prefabFactory = prefabFactory;
        this.customBootstrappersByTechType = customBootstrappersByTechType;
        this.prefabPlaceholderGroupsByClassId = prefabPlaceholderGroupsByClassId;
        this.seed = seed;
        batchCellsParser = new BatchCellsParser(entitySpawnPointFactory, serializer);
    }

    public bool IsBatchSpawned(NitroxInt3 batchId)
    {
        lock (parsedBatches)
        {
            return parsedBatches.Contains(batchId);
        }
    }

    public List<Entity> LoadUnspawnedEntities(NitroxInt3 batchId, bool fullCacheCreation = false)
    {
        lock (parsedBatches)
        {
            if (parsedBatches.Contains(batchId))
            {
                return new List<Entity>();
            }

            parsedBatches.Add(batchId);
        }

        DeterministicGenerator deterministicBatchGenerator = new DeterministicGenerator(seed, batchId);
        List<EntitySpawnPoint> spawnPoints = batchCellsParser.ParseBatchData(batchId);
        List<Entity> entities = SpawnEntities(spawnPoints, deterministicBatchGenerator);

        if (entities.Count == 0)
        {
            lock (emptyBatchesLock)
            {
                emptyBatches.Add(batchId);
            }
        }
        else if (!fullCacheCreation)
        {
            Log.Info($"Spawning {entities.Count} entities from {spawnPoints.Count} spawn points in batch {batchId}");
        }

        for (int x = 0; x < entities.Count; x++) // Throws on duplicate Entities already but nice to know which ones
        {
            for (int y = 0; y < entities.Count; y++)
            {
                if (entities[x] == entities[y] && x != y)
                {
                    Log.Error($"Duplicate Entity detected! {entities[x]}");
                }
            }
        }

        return entities;
    }

    private IEnumerable<Entity> SpawnEntitiesUsingRandomDistribution(EntitySpawnPoint entitySpawnPoint, List<UwePrefab> prefabs, DeterministicGenerator deterministicBatchGenerator, Entity parentEntity = null)
    {
        List<UwePrefab> allowedPrefabs = FilterAllowedPrefabs(prefabs, entitySpawnPoint);

        float rollingProbabilityDensity = allowedPrefabs.Sum(prefab => prefab.Probability / entitySpawnPoint.Density);

        if (rollingProbabilityDensity <= 0)
        {
            yield break;
        }

        float randomNumber = (float)deterministicBatchGenerator.NextDouble();
        if (rollingProbabilityDensity > 1f)
        {
            randomNumber *= rollingProbabilityDensity;
        }

        float rollingProbability = 0;

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

    private IEnumerable<Entity> SpawnEntitiesStaticly(EntitySpawnPoint entitySpawnPoint, DeterministicGenerator deterministicBatchGenerator, WorldEntity parentEntity = null)
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

    private IEnumerable<Entity> CreateEntityWithChildren(EntitySpawnPoint entitySpawnPoint, NitroxVector3 scale, NitroxTechType techType, int cellLevel, string classId, DeterministicGenerator deterministicBatchGenerator, Entity parentEntity = null)
    {
        WorldEntity spawnedEntity;

        if (classId == CellRootEntity.CLASS_ID)
        {
            spawnedEntity = new CellRootEntity(entitySpawnPoint.LocalPosition,
                                               entitySpawnPoint.LocalRotation,
                                               scale,
                                               techType,
                                               cellLevel,
                                               classId,
                                               true,
                                               deterministicBatchGenerator.NextId());
        }
        else
        {
            spawnedEntity = new WorldEntity(entitySpawnPoint.LocalPosition,
                                            entitySpawnPoint.LocalRotation,
                                            scale,
                                            techType,
                                            cellLevel,
                                            classId,
                                            true,
                                            deterministicBatchGenerator.NextId(),
                                            parentEntity);
        }

        if (!TryCreatePrefabPlaceholdersGroupWithChildren(ref spawnedEntity, classId, deterministicBatchGenerator))
        {
            spawnedEntity.ChildEntities = SpawnEntities(entitySpawnPoint.Children, deterministicBatchGenerator, spawnedEntity);
        }

        if (customBootstrappersByTechType.TryGetValue(techType, out IEntityBootstrapper bootstrapper))
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

    private List<Entity> SpawnEntities(List<EntitySpawnPoint> entitySpawnPoints, DeterministicGenerator deterministicBatchGenerator, WorldEntity parentEntity = null)
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

    /// <summary>
    /// Check to see if this entity is a PrefabPlaceholderGroup.
    /// If it is, we want to add the PrefabPlaceholders that would be spawned here.
    /// This is suppressed on the client so we don't get virtual entities that the server doesn't know about.
    /// </summary>
    /// <returns>If this Entity is a PrefabPlaceholdersGroup</returns>
    private bool TryCreatePrefabPlaceholdersGroupWithChildren(ref WorldEntity entity, string classId, DeterministicGenerator deterministicBatchGenerator)
    {
        if (!prefabPlaceholderGroupsByClassId.TryGetValue(classId, out PrefabPlaceholdersGroupAsset group))
        {
            return false;
        }

        List<Entity> placeholders = new(group.PrefabPlaceholders.Length);
        entity = new PlaceholderGroupWorldEntity(entity, placeholders);

        for (int i = 0; i < group.PrefabPlaceholders.Length; i++)
        {
            PrefabPlaceholderAsset placeholder = group.PrefabPlaceholders[i];

            PrefabChildEntity prefabChild = new(deterministicBatchGenerator.NextId(), null, NitroxTechType.None, i, null, entity.Id);
            placeholders.Add(prefabChild);

            if (placeholder.EntitySlot == null)
            {
                prefabChild.ChildEntities.Add(new PrefabPlaceholderEntity(deterministicBatchGenerator.NextId(), group.TechType, prefabChild.Id));
            }
            else
            {
                Entity entitySlotNullableEntity = SpawnEntitySlotEntities(placeholder.EntitySlot, deterministicBatchGenerator, entity.AbsoluteEntityCell, prefabChild);

                if (entitySlotNullableEntity != null)
                {
                    prefabChild.ChildEntities.Add(entitySlotNullableEntity);
                }
            }
        }

        return true;
    }

    private Entity SpawnEntitySlotEntities(NitroxEntitySlot entitySlot, DeterministicGenerator deterministicBatchGenerator, AbsoluteEntityCell cell, PrefabChildEntity parentEntity)
    {
        List<UwePrefab> prefabs = prefabFactory.GetPossiblePrefabs(entitySlot.BiomeType);
        List<Entity> entities = new();

        if (prefabs.Count > 0)
        {
            EntitySpawnPoint entitySpawnPoint = new EntitySpawnPoint(cell, NitroxVector3.Zero, NitroxQuaternion.Identity, entitySlot.AllowedTypes.ToList(), 1f, entitySlot.BiomeType);
            entities.AddRange(SpawnEntitiesUsingRandomDistribution(entitySpawnPoint, prefabs, deterministicBatchGenerator, parentEntity));
        }

        return entities.FirstOrDefault();
    }

}
