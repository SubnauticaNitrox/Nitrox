using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private readonly WorldEntitySpawnerResolver spawnerResolver;
    private readonly DefaultWorldEntitySpawner defaultSpawner;

    public PlaceholderGroupWorldEntitySpawner(WorldEntitySpawnerResolver spawnerResolver, DefaultWorldEntitySpawner defaultSpawner)
    {
        this.spawnerResolver = spawnerResolver;
        this.defaultSpawner = defaultSpawner;
    }

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        TaskResult<Optional<GameObject>> prefabPlaceholderGroupTaskResult = new();
        if (!defaultSpawner.SpawnSync(entity, parent, cellRoot, prefabPlaceholderGroupTaskResult))
        {
            yield return defaultSpawner.SpawnAsync(entity, parent, cellRoot, prefabPlaceholderGroupTaskResult);
        }

        Optional<GameObject> prefabPlaceholderGroupGameObject = prefabPlaceholderGroupTaskResult.Get();

        if (!prefabPlaceholderGroupGameObject.HasValue)
        {
            result.Set(Optional.Empty);
            yield break;
        }

        if (entity is not PlaceholderGroupWorldEntity placeholderGroupEntity)
        {
            result.Set(Optional.Empty);
            yield break;
        }

        result.Set(prefabPlaceholderGroupGameObject);

        // Spawning PrefabPlaceholders as siblings to the group
        PrefabPlaceholdersGroup prefabPlaceholderGroup = prefabPlaceholderGroupGameObject.Value.GetComponent<PrefabPlaceholdersGroup>();

        // Spawning all children iteratively
        Stack<Entity> stack = new(placeholderGroupEntity.ChildEntities.OfType<PrefabChildEntity>());

        TaskResult<Optional<GameObject>> childResult = new();
        Dictionary<NitroxId, Optional<GameObject>> parentById = new();
        IEnumerator asyncInstructions;
        while (stack.Count > 0)
        {
            childResult.Set(Optional.Empty);
            Entity current = stack.Pop();
            switch (current)
            {
                // First layer of children under PlaceholderGroupWorldEntity
                case PrefabChildEntity placeholderSlot:
                    // Entity was a slot not spawned, picked up, or removed
                    if (placeholderSlot.ChildEntities.Count == 0)
                    {
                        continue;
                    }

                    PrefabPlaceholder prefabPlaceholder = prefabPlaceholderGroup.prefabPlaceholders[placeholderSlot.ComponentIndex];
                    Entity slotEntity = placeholderSlot.ChildEntities[0];

                    switch (slotEntity)
                    {
                        case PrefabPlaceholderEntity placeholder:
                            if (!SpawnChildPlaceholderSync(prefabPlaceholder, placeholder, childResult, out asyncInstructions))
                            {
                                yield return asyncInstructions;
                            }
                            break;
                        case WorldEntity worldEntity:
                            if (!SpawnWorldEntityChildSync(worldEntity, cellRoot, Optional.Of(prefabPlaceholder.transform.parent.gameObject), childResult, out asyncInstructions))
                            {
                                yield return asyncInstructions;
                            }
                            break;
                        default:
                            Log.Debug(placeholderSlot.ChildEntities.Count > 0 ? $"Unhandled child type {placeholderSlot.ChildEntities[0]}" : "Child was null");
                            break;
                    }
                    break;

                // Other layers under PlaceholderGroupWorldEntity's children
                case WorldEntity worldEntity:
                    Optional<GameObject> slotParent = parentById[worldEntity.ParentId];

                    if (!SpawnWorldEntityChildSync(worldEntity, cellRoot, slotParent, childResult, out asyncInstructions))
                    {
                        yield return asyncInstructions;
                    }
                    break;
            }

            if (!childResult.value.HasValue)
            {
                Log.Error($"[{nameof(PlaceholderGroupWorldEntitySpawner)}] Spawning of child failed {current}");
                continue;
            }

            EntityMetadataProcessor.ApplyMetadata(childResult.value.Value, current.Metadata);
            // Adding children to be spawned by this loop
            foreach (WorldEntity slotEntityChild in current.ChildEntities.OfType<WorldEntity>())
            {
                stack.Push(slotEntityChild);
            }
            parentById[current.Id] = childResult.value;
        }
    }

    public bool SpawnsOwnChildren() => true;

    private IEnumerator SpawnChildPlaceholderAsync(PrefabPlaceholder prefabPlaceholder, PrefabPlaceholderEntity entity, TaskResult<Optional<GameObject>> result)
    {
        TaskResult<GameObject> goResult = new();
        yield return DefaultWorldEntitySpawner.CreateGameObject(TechType.None, prefabPlaceholder.prefabClassId, goResult);
        
        if (goResult.value)
        {
            SetupPlaceholder(goResult.value, prefabPlaceholder, entity, result);
        }
    }

    private bool SpawnChildPlaceholderSync(PrefabPlaceholder prefabPlaceholder, PrefabPlaceholderEntity entity, TaskResult<Optional<GameObject>> result, out IEnumerator asyncInstructions)
    {
        if (!DefaultWorldEntitySpawner.TryCreateGameObjectSync(TechType.None, prefabPlaceholder.prefabClassId, out GameObject gameObject))
        {
            asyncInstructions = SpawnChildPlaceholderAsync(prefabPlaceholder, entity, result);
            return false;
        }

        SetupPlaceholder(gameObject, prefabPlaceholder, entity, result);
        asyncInstructions = null;
        return true;
    }

    private void SetupPlaceholder(GameObject gameObject, PrefabPlaceholder prefabPlaceholder, PrefabPlaceholderEntity entity, TaskResult<Optional<GameObject>> result)
    {
        try
        {
            gameObject.transform.SetParent(prefabPlaceholder.transform.parent, false);
            gameObject.transform.localPosition = prefabPlaceholder.transform.localPosition;
            gameObject.transform.localRotation = prefabPlaceholder.transform.localRotation;

            NitroxEntity.SetNewId(gameObject, entity.Id);
            result.Set(gameObject);
        }
        catch (Exception e)
        {
            Log.Error(e);
            result.Set(Optional.Empty);
        }
    }

    private IEnumerator SpawnWorldEntityChildAsync(WorldEntity worldEntity, EntityCell cellRoot, Optional<GameObject> parent, TaskResult<Optional<GameObject>> worldEntityResult)
    {
        IWorldEntitySpawner spawner = spawnerResolver.ResolveEntitySpawner(worldEntity);
        yield return spawner.SpawnAsync(worldEntity, parent, cellRoot, worldEntityResult);

        if (worldEntityResult.value.HasValue)
        {
            worldEntityResult.value.Value.transform.localPosition = worldEntity.Transform.LocalPosition.ToUnity();
            worldEntityResult.value.Value.transform.localRotation = worldEntity.Transform.LocalRotation.ToUnity();
        }
    }

    private bool SpawnWorldEntityChildSync(WorldEntity worldEntity, EntityCell cellRoot, Optional<GameObject> parent, TaskResult<Optional<GameObject>> worldEntityResult, out IEnumerator asyncInstructions)
    {
        IWorldEntitySpawner spawner = spawnerResolver.ResolveEntitySpawner(worldEntity);

        if (spawner is not IWorldEntitySyncSpawner syncSpawner ||
            !syncSpawner.SpawnSync(worldEntity, parent, cellRoot, worldEntityResult) ||
            !worldEntityResult.value.HasValue)
        {
            asyncInstructions = SpawnWorldEntityChildAsync(worldEntity, cellRoot, parent, worldEntityResult);
            return false;
        }

        worldEntityResult.value.Value.transform.localPosition = worldEntity.Transform.LocalPosition.ToUnity();
        worldEntityResult.value.Value.transform.localRotation = worldEntity.Transform.LocalRotation.ToUnity();
        asyncInstructions = null;
        return true;
    }
}
