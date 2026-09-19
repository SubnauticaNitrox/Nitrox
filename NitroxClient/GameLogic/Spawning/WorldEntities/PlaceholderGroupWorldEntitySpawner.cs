using System.Collections;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using NitroxClient.GameLogic.Spawning.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

/// <remarks>
/// This spawner can't hold a SpawnSync function because it is also responsible for spawning its children
/// so the <see cref="SpawnAsync"/> function will still use sync spawning when possible and fall back to async when required.
/// </remarks>
internal sealed class PlaceholderGroupWorldEntitySpawner(Entities entities, WorldEntitySpawnerResolver spawnerResolver, DefaultWorldEntitySpawner defaultWorldEntitySpawner, EntityMetadataManager entityMetadataManager, PrefabPlaceholderEntitySpawner prefabPlaceholderEntitySpawner) : IWorldEntitySpawner
{
    private readonly Entities entities = entities;
    private readonly WorldEntitySpawnerResolver spawnerResolver = spawnerResolver;
    private readonly DefaultWorldEntitySpawner defaultWorldEntitySpawner = defaultWorldEntitySpawner;
    private readonly EntityMetadataManager entityMetadataManager = entityMetadataManager;
    private readonly PrefabPlaceholderEntitySpawner prefabPlaceholderEntitySpawner = prefabPlaceholderEntitySpawner;

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not PlaceholderGroupWorldEntity placeholderGroupEntity)
        {
            Log.Error($"[{nameof(PlaceholderGroupWorldEntitySpawner)}] Can't spawn {entity.Id} of type {entity.GetType()} because it is not a {nameof(PlaceholderGroupWorldEntity)}");
            yield break;
        }

        TaskResult<Optional<GameObject>> prefabPlaceholderGroupTaskResult = new();

        if (!DefaultWorldEntitySpawner.TryCreateGameObjectSync(entity.TechType.ToUnity(), entity.ClassId, entity.Id, out GameObject groupObject))
        {
            Log.ErrorOnce($"[{nameof(PlaceholderGroupWorldEntitySpawner)}] Could not find a prefab for {entity.Id} [classId: {entity.ClassId}, TechType: {entity.TechType}]");
            yield break;
        }
        LargeWorldEntity largeWorldEntity = groupObject.GetComponent<LargeWorldEntity>();
        PrefabPlaceholdersGroup prefabPlaceholderGroup = groupObject.GetComponent<PrefabPlaceholdersGroup>();

        DefaultWorldEntitySpawner.SetupObject(entity, parent, groupObject, entity.TechType.ToUnity(), false);

        // Prevent the entity from disappearing because of parent cell going to sleep until it's fully spawned
        if (!parent.HasValue)
        {
            largeWorldEntity.enabled = false;
        }

        // Spawning all children iteratively
        Stack<Entity> stack = new(placeholderGroupEntity.ChildEntities);

        TaskResult<Optional<GameObject>> childResult = new();
        Dictionary<NitroxId, GameObject> parentById = new()
        {
            { entity.Id, groupObject }
        };

        while (stack.Count > 0)
        {
            childResult.Set(Optional.Empty);
            Entity current = stack.Pop();
            switch (current)
            {
                case PrefabPlaceholderEntity prefabEntity:
                    if (!prefabPlaceholderEntitySpawner.SpawnSync(prefabEntity, groupObject, cellRoot, childResult))
                    {
                        Log.Error($"[{nameof(PlaceholderGroupWorldEntitySpawner)}] Could not spawn child entity {prefabEntity}");
                    }
                    break;

                case PlaceholderGroupWorldEntity groupEntity:
                    PrefabPlaceholder placeholder = prefabPlaceholderGroup.prefabPlaceholders[groupEntity.ComponentIndex];
                    yield return SpawnAsync(groupEntity, placeholder.transform.parent.gameObject, cellRoot, childResult);
                    entities.RefreshTimeUntilNextYield();
                    break;

                case WorldEntity worldEntity:
                    if (!SpawnWorldEntityChildSync(worldEntity, cellRoot, parentById.GetOrDefault(current.ParentId, null), childResult, out IEnumerator asyncInstructions))
                    {
                        yield return asyncInstructions;
                        entities.RefreshTimeUntilNextYield();
                    }
                    break;

                default:
                    Log.Error($"[{nameof(PlaceholderGroupWorldEntitySpawner)}] Can't spawn a child entity which is not a WorldEntity: {current}");
                    continue;
            }

            if (!childResult.value.HasValue)
            {
                Log.Error($"[{nameof(PlaceholderGroupWorldEntitySpawner)}] Spawning of child failed {current}");
                continue;
            }

            GameObject childObject = childResult.value.Value;
            entities.OnEntitySpawned(current, childObject);
            parentById[current.Id] = childObject;

            // PlaceholderGroupWorldEntity's children spawning is already handled by this function which is called recursively
            if (current is not PlaceholderGroupWorldEntity)
            {
                // Adding children to be spawned by this loop            
                foreach (Entity slotEntityChild in current.ChildEntities)
                {
                    stack.Push(slotEntityChild);
                }
            }

            if (entities.ShouldSkipFrame)
            {
                yield return null;
                entities.RefreshTimeUntilNextYield();
            }
        }

        // Handle setting isKinematic on Floating Stones
        prefabPlaceholderGroup.OnPrefabGroupSpawned?.Invoke();

        if (!parent.HasValue)
        {
            largeWorldEntity.enabled = true;
            LargeWorldEntity.Register(groupObject);
        }

        result.Set(groupObject);
    }

    public bool SpawnsOwnChildren() => true;

    private IEnumerator SpawnWorldEntityChildAsync(WorldEntity worldEntity, EntityCell cellRoot, GameObject parent, TaskResult<Optional<GameObject>> worldEntityResult)
    {
        IWorldEntitySpawner spawner = spawnerResolver.ResolveEntitySpawner(worldEntity);
        yield return spawner.SpawnAsync(worldEntity, parent, cellRoot, worldEntityResult);
        if (!worldEntityResult.value.HasValue)
        {
            yield break;
        }
        GameObject spawnedObject = worldEntityResult.value.Value;

        spawnedObject.transform.localPosition = worldEntity.Transform.LocalPosition.ToUnity();
        spawnedObject.transform.localRotation = worldEntity.Transform.LocalRotation.ToUnity();
        spawnedObject.transform.localScale = worldEntity.Transform.LocalScale.ToUnity();
    }

    private bool SpawnWorldEntityChildSync(WorldEntity worldEntity, EntityCell cellRoot, GameObject parent, TaskResult<Optional<GameObject>> worldEntityResult, out IEnumerator asyncInstructions)
    {
        IWorldEntitySpawner spawner = spawnerResolver.ResolveEntitySpawner(worldEntity);

        if (spawner is not IWorldEntitySyncSpawner syncSpawner ||
            !syncSpawner.SpawnSync(worldEntity, parent, cellRoot, worldEntityResult) ||
            !worldEntityResult.value.HasValue)
        {
            asyncInstructions = SpawnWorldEntityChildAsync(worldEntity, cellRoot, parent, worldEntityResult);
            return false;
        }
        GameObject spawnedObject = worldEntityResult.value.Value;

        spawnedObject.transform.localPosition = worldEntity.Transform.LocalPosition.ToUnity();
        spawnedObject.transform.localRotation = worldEntity.Transform.LocalRotation.ToUnity();
        spawnedObject.transform.localScale = worldEntity.Transform.LocalScale.ToUnity();
        asyncInstructions = null;
        return true;
    }
}
