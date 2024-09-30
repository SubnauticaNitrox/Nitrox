using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxServer.GameLogic.Unlockables;
using NitroxServer.Helper;
using NitroxServer.Resources;
using NitroxServer.Serialization;

namespace NitroxServer.GameLogic.Entities.Spawning;

public class BatchEntitySpawner : IEntitySpawner
{
    private readonly BatchCellsParser batchCellsParser;

    private readonly HashSet<NitroxInt3> emptyBatches = new();
    private readonly Dictionary<string, PrefabPlaceholdersGroupAsset> placeholdersGroupsByClassId;
    private readonly IUwePrefabFactory prefabFactory;
    private readonly IEntityBootstrapperManager entityBootstrapperManager;
    private readonly PDAStateData pdaStateData;

    private readonly string seed;

    private readonly IUweWorldEntityFactory worldEntityFactory;

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

    private static readonly NitroxQuaternion prefabZUpRotation = NitroxQuaternion.FromEuler(new(-90f, 0f, 0f));

    public BatchEntitySpawner(EntitySpawnPointFactory entitySpawnPointFactory, IUweWorldEntityFactory worldEntityFactory, IUwePrefabFactory prefabFactory, List<NitroxInt3> loadedPreviousParsed, ServerProtoBufSerializer serializer,
                              IEntityBootstrapperManager entityBootstrapperManager, Dictionary<string, PrefabPlaceholdersGroupAsset> placeholdersGroupsByClassId, PDAStateData pdaStateData, string seed)
    {
        parsedBatches = new HashSet<NitroxInt3>(loadedPreviousParsed);
        this.worldEntityFactory = worldEntityFactory;
        this.prefabFactory = prefabFactory;
        this.entityBootstrapperManager = entityBootstrapperManager;
        this.placeholdersGroupsByClassId = placeholdersGroupsByClassId;
        this.pdaStateData = pdaStateData;
        batchCellsParser = new BatchCellsParser(entitySpawnPointFactory, serializer);
        this.seed = seed;
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

    /// <inheritdoc cref="CreateEntityWithChildren" />
    private IEnumerable<Entity> SpawnEntitiesUsingRandomDistribution(EntitySpawnPoint entitySpawnPoint, List<UwePrefab> prefabs, DeterministicGenerator deterministicBatchGenerator, Entity parentEntity = null)
    {
        // See CSVEntitySpawner.GetPrefabForSlot for reference
        List<UwePrefab> allowedPrefabs = FilterAllowedPrefabs(prefabs, entitySpawnPoint, out float fragmentProbability, out float completeFragmentProbability);

        bool areFragmentProbabilitiesNonNull = fragmentProbability > 0f && completeFragmentProbability > 0f;
        float probabilityMultiplier = areFragmentProbabilitiesNonNull ? (completeFragmentProbability + fragmentProbability) / fragmentProbability : 1f;
        float weightedFragmentProbability = 0f;
        for (int i = 0; i < allowedPrefabs.Count; i++)
        {
            UwePrefab prefab = allowedPrefabs[i];
            if (areFragmentProbabilitiesNonNull && prefab.IsFragment)
            {
                prefab = prefab with { Probability = prefab.Probability * probabilityMultiplier };
                allowedPrefabs[i] = prefab;
            }
            weightedFragmentProbability += prefab.Probability;
        }

        UwePrefab chosenPrefab = default;
        if (weightedFragmentProbability > 0f)
        {
            float probabilityThreshold = XORRandom.NextFloat();
            if (weightedFragmentProbability > 1f)
            {
                probabilityThreshold *= weightedFragmentProbability;
            }
            float currentProbability = 0f;
            foreach (UwePrefab prefab in allowedPrefabs)
            {
                currentProbability += prefab.Probability;
                if (currentProbability >= probabilityThreshold)
                {
                    chosenPrefab = prefab;
                    break;
                }
            }
        }

        if (chosenPrefab.Count == 0)
        {
            yield break;
        }

        if (worldEntityFactory.TryFind(chosenPrefab.ClassId, out UweWorldEntity uweWorldEntity))
        {
            for (int i = 0; i < chosenPrefab.Count; i++)
            {
                // Random position in sphere is only possible after first spawn, see EntitySlot.Spawn
                IEnumerable<Entity> entities = CreateEntityWithChildren(entitySpawnPoint,
                                                                        chosenPrefab.ClassId,
                                                                        uweWorldEntity.TechType,
                                                                        uweWorldEntity.PrefabZUp,
                                                                        uweWorldEntity.CellLevel,
                                                                        uweWorldEntity.LocalScale,
                                                                        deterministicBatchGenerator,
                                                                        parentEntity,
                                                                        i > 0);
                foreach (Entity entity in entities)
                {
                    yield return entity;
                }
            }
        }
    }

    private List<UwePrefab> FilterAllowedPrefabs(List<UwePrefab> prefabs, EntitySpawnPoint entitySpawnPoint, out float fragmentProbability, out float completeFragmentProbability)
    {
        List<UwePrefab> allowedPrefabs = new();

        fragmentProbability = 0;
        completeFragmentProbability = 0;
        for (int i = 0; i < prefabs.Count; i++)
        {
            UwePrefab prefab = prefabs[i];
            // Adapted code from the while loop in CSVEntitySpawner.GetPrefabForSlot 
            if (prefab.ClassId != "None" && worldEntityFactory.TryFind(prefab.ClassId, out UweWorldEntity uweWorldEntity) &&
                entitySpawnPoint.AllowedTypes.Contains(uweWorldEntity.SlotType))
            {
                float weightedProbability = prefab.Probability / entitySpawnPoint.Density;
                if (weightedProbability > 0)
                {
                    if (prefab.IsFragment)
                    {
                        if (pdaStateData.ScannerComplete.Contains(uweWorldEntity.TechType))
                        {
                            completeFragmentProbability += weightedProbability;
                            continue;
                        }
                        else
                        {
                            fragmentProbability += weightedProbability;
                        }
                    }
                    prefab = prefab with { Probability = weightedProbability };
                    allowedPrefabs.Add(prefab);
                }
            }
        }

        return allowedPrefabs;
    }

    /// <summary>
    /// Spawns the regular (can be children of PrefabPlaceholdersGroup) which are always the same thus context independent.
    /// </summary>
    /// <inheritdoc cref="CreateEntityWithChildren" />
    private IEnumerable<Entity> SpawnEntitiesStaticly(EntitySpawnPoint entitySpawnPoint, DeterministicGenerator deterministicBatchGenerator, WorldEntity parentEntity = null)
    {
        if (worldEntityFactory.TryFind(entitySpawnPoint.ClassId, out UweWorldEntity uweWorldEntity))
        {
            // prefabZUp should not be taken into account for statically spawned entities
            IEnumerable<Entity> entities = CreateEntityWithChildren(entitySpawnPoint,
                                                                    entitySpawnPoint.ClassId,
                                                                    uweWorldEntity.TechType,
                                                                    false,
                                                                    uweWorldEntity.CellLevel,
                                                                    entitySpawnPoint.Scale,
                                                                    deterministicBatchGenerator,
                                                                    parentEntity);
            foreach (Entity entity in entities)
            {
                yield return entity;
            }
        }
    }

    /// <returns>The first entity is a <see cref="WorldEntity"/> and the following are its children</returns>
    private IEnumerable<Entity> CreateEntityWithChildren(EntitySpawnPoint entitySpawnPoint, string classId, NitroxTechType techType, bool prefabZUp, int cellLevel, NitroxVector3 localScale, DeterministicGenerator deterministicBatchGenerator, Entity parentEntity = null, bool randomPosition = false)
    {
        WorldEntity spawnedEntity;
        NitroxVector3 position = entitySpawnPoint.LocalPosition;
        NitroxQuaternion rotation = entitySpawnPoint.LocalRotation;
        if (prefabZUp)
        {
            // See EntitySlot.SpawnVirtualEntities use of WorldEntityInfo.prefabZUp
            rotation *= prefabZUpRotation;
        }
        if (randomPosition)
        {
            position += XORRandom.NextInsideSphere(4f);
        }

        if (classId == CellRootEntity.CLASS_ID)
        {
            spawnedEntity = new CellRootEntity(position,
                                               rotation,
                                               localScale,
                                               techType,
                                               cellLevel,
                                               classId,
                                               true,
                                               deterministicBatchGenerator.NextId());
        }
        else
        {
            spawnedEntity = new WorldEntity(position,
                                            rotation,
                                            localScale,
                                            techType,
                                            cellLevel,
                                            classId,
                                            true,
                                            deterministicBatchGenerator.NextId(),
                                            parentEntity);
        }

        // See EntitySlotsPlaceholder.Spawn
        if (!TryCreatePrefabPlaceholdersGroupWithChildren(ref spawnedEntity, classId, deterministicBatchGenerator))
        {
            spawnedEntity.ChildEntities = SpawnEntities(entitySpawnPoint.Children, deterministicBatchGenerator, spawnedEntity);
        }

        entityBootstrapperManager.PrepareEntityIfRequired(ref spawnedEntity, deterministicBatchGenerator);

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
        List<Entity> entities = new();
        foreach (EntitySpawnPoint esp in entitySpawnPoints)
        {
            if (esp is SerializedEntitySpawnPoint serializedEsp)
            {
                // We add the cell's coordinate because this entity isn't parented so it needs to know about its global position
                NitroxTransform transform = new(serializedEsp.LocalPosition + serializedEsp.AbsoluteEntityCell.Position, serializedEsp.LocalRotation, serializedEsp.Scale);
                SerializedWorldEntity entity = new(serializedEsp.SerializedComponents, serializedEsp.Layer, transform, deterministicBatchGenerator.NextId(), parentEntity?.Id, serializedEsp.AbsoluteEntityCell);
                entities.Add(entity);
                continue;
            }

            if (esp.Density > 0)
            {
                if (prefabFactory.TryGetPossiblePrefabs(esp.BiomeType, out List<UwePrefab> prefabs) && prefabs.Count > 0)
                {
                    entities.AddRange(SpawnEntitiesUsingRandomDistribution(esp, prefabs, deterministicBatchGenerator, parentEntity));
                }
                else if (!string.IsNullOrEmpty(esp.ClassId))
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
        if (!placeholdersGroupsByClassId.TryGetValue(classId, out PrefabPlaceholdersGroupAsset groupAsset))
        {
            return false;
        }

        entity = new PlaceholderGroupWorldEntity(entity);

        // Adapted from PrefabPlaceholdersGroup.Spawn
        for (int i = 0; i < groupAsset.PrefabAssets.Length; i++)
        {
            // Fix positioning of children
            IPrefabAsset prefabAsset = groupAsset.PrefabAssets[i];

            // Two cases, either the PrefabPlaceholder holds a visible GameObject or an EntitySlot (a MB which has a chance of spawning a prefab)
            if (prefabAsset is PrefabPlaceholderAsset placeholderAsset && placeholderAsset.EntitySlot != null)
            {
                WorldEntity spawnedEntity = SpawnPrefabAssetInEntitySlot(placeholderAsset.Transform, placeholderAsset.EntitySlot, deterministicBatchGenerator, entity.AbsoluteEntityCell, entity);

                if (spawnedEntity != null)
                {
                    // Spawned child will not be of the same type as the current prefabAsset
                    if (placeholdersGroupsByClassId.ContainsKey(spawnedEntity.ClassId))
                    {
                        spawnedEntity = new PlaceholderGroupWorldEntity(spawnedEntity, i);
                    }
                    else
                    {
                        spawnedEntity = new PrefabPlaceholderEntity(spawnedEntity, true, i);
                    }
                    entity.ChildEntities.Add(spawnedEntity);
                }
            }
            else
            {
                // Regular visible GameObject
                string prefabClassId = prefabAsset.ClassId;
                if (prefabAsset is PrefabPlaceholderRandomAsset randomAsset && randomAsset.ClassIds.Count > 0)
                {
                    int randomIndex = XORRandom.NextIntRange(0, randomAsset.ClassIds.Count);
                    prefabClassId = randomAsset.ClassIds[randomIndex];
                }

                EntitySpawnPoint esp = new(entity.AbsoluteEntityCell, prefabAsset.Transform.LocalPosition, prefabAsset.Transform.LocalRotation, prefabAsset.Transform.LocalScale, prefabClassId);
                WorldEntity spawnedEntity = (WorldEntity)SpawnEntitiesStaticly(esp, deterministicBatchGenerator, entity).First();
                if (prefabAsset is PrefabPlaceholdersGroupAsset)
                {
                    spawnedEntity = new PlaceholderGroupWorldEntity(spawnedEntity, i);
                }
                else
                {
                    spawnedEntity = new PrefabPlaceholderEntity(spawnedEntity, false, i);
                }

                entity.ChildEntities.Add(spawnedEntity);
            }
        }

        return true;
    }

    private WorldEntity SpawnPrefabAssetInEntitySlot(NitroxTransform transform, NitroxEntitySlot entitySlot, DeterministicGenerator deterministicBatchGenerator, AbsoluteEntityCell cell, Entity parentEntity)
    {
        if (!prefabFactory.TryGetPossiblePrefabs(entitySlot.BiomeType, out List<UwePrefab> prefabs) || prefabs.Count == 0)
        {
            return null;
        }
        List<Entity> entities = new();

        EntitySpawnPoint entitySpawnPoint = new(cell, transform.LocalPosition, transform.LocalRotation, entitySlot.AllowedTypes.ToList(), 1f, entitySlot.BiomeType);
        entities.AddRange(SpawnEntitiesUsingRandomDistribution(entitySpawnPoint, prefabs, deterministicBatchGenerator, parentEntity));
        if (entities.Count > 0)
        {
            return (WorldEntity)entities[0];
        }
        return null;
    }
}
