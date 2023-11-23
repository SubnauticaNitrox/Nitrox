using System.Collections;
using System.Collections.Generic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

/// <remarks>
/// This spawner can't hold a SpawnSync function because it is also responsible for spawning its children
/// so the <see cref="SpawnAsync"/> function will still use sync spawning when possible and fall back to async when required.
/// </remarks>
public class PlaceholderGroupWorldEntitySpawner : IWorldEntitySpawner
{
    private readonly Entities entities;
    private readonly WorldEntitySpawnerResolver spawnerResolver;
    private readonly DefaultWorldEntitySpawner defaultSpawner;

    public PlaceholderGroupWorldEntitySpawner(Entities entities, WorldEntitySpawnerResolver spawnerResolver, DefaultWorldEntitySpawner defaultSpawner)
    {
        this.entities = entities;
        this.spawnerResolver = spawnerResolver;
        this.defaultSpawner = defaultSpawner;
    }

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not PlaceholderGroupWorldEntity placeholderGroupEntity)
        {
            yield break;
        }

        TaskResult<Optional<GameObject>> prefabPlaceholderGroupTaskResult = new();
        if (!defaultSpawner.SpawnSync(entity, parent, cellRoot, prefabPlaceholderGroupTaskResult))
        {
            yield return defaultSpawner.SpawnAsync(entity, parent, cellRoot, prefabPlaceholderGroupTaskResult);
        }

        Optional<GameObject> prefabPlaceholderGroupGameObject = prefabPlaceholderGroupTaskResult.Get();

        if (!prefabPlaceholderGroupGameObject.HasValue)
        {
            yield break;
        }

        GameObject groupObject = prefabPlaceholderGroupGameObject.Value;
        // Spawning PrefabPlaceholders as siblings to the group
        PrefabPlaceholdersGroup prefabPlaceholderGroup = groupObject.GetComponent<PrefabPlaceholdersGroup>();

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
                    PrefabPlaceholder placeholder = prefabPlaceholderGroup.prefabPlaceholders[prefabEntity.ComponentIndex];
                    if (!SpawnWorldEntityChildSync(prefabEntity, cellRoot, placeholder.transform.parent.gameObject, childResult, out IEnumerator asyncInstructions))
                    {
                        yield return asyncInstructions;
                    }
                    break;

                case PlaceholderGroupWorldEntity groupEntity:
                    placeholder = prefabPlaceholderGroup.prefabPlaceholders[groupEntity.ComponentIndex];
                    yield return SpawnAsync(groupEntity, placeholder.transform.parent.gameObject, cellRoot, childResult);
                    break;
                default:
                    Log.Error($"[{nameof(PlaceholderGroupWorldEntitySpawner)}] Found a child entity to spawn with an unmanaged type: {entity.GetType()}");
                    break;
            }

            if (!childResult.value.HasValue)
            {
                Log.Error($"[{nameof(PlaceholderGroupWorldEntitySpawner)}] Spawning of child failed {current}");
                continue;
            }

            GameObject childObject = childResult.value.Value;
            entities.MarkAsSpawned(current);
            parentById[current.Id] = childObject;
            EntityMetadataProcessor.ApplyMetadata(childObject, current.Metadata);
            if (current is not PlaceholderGroupWorldEntity)
            {
                continue;
            }
            NitroxEntity.SetNewId(childObject, current.Id);
            // Adding children to be spawned by this loop
            foreach (Entity slotEntityChild in current.ChildEntities)
            {
                stack.Push(slotEntityChild);
            }
        }

        result.Set(prefabPlaceholderGroupGameObject);
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
