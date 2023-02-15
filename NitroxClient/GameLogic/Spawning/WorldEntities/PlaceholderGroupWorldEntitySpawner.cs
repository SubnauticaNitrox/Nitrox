using System.Collections;
using System.Linq;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

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
        yield return defaultSpawner.SpawnAsync(entity, parent, cellRoot, prefabPlaceholderGroupTaskResult);
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

        // Spawning PrefabPlaceholders
        PrefabPlaceholdersGroup prefabPlaceholderGroup = prefabPlaceholderGroupGameObject.Value.GetComponent<PrefabPlaceholdersGroup>();

        foreach (PrefabChildEntity placeholderSlot in placeholderGroupEntity.ChildEntities.Cast<PrefabChildEntity>())
        {
            if (placeholderSlot.ChildEntities.Count == 0) //Entity was a slot not spawned, picked up, or removed
            {
                continue;
            }

            PrefabPlaceholder prefabPlaceholder = prefabPlaceholderGroup.prefabPlaceholders[placeholderSlot.ComponentIndex];

            TaskResult<Optional<GameObject>> childResult = new();

            Entity slotEntity = placeholderSlot.ChildEntities[0];

            switch (slotEntity)
            {
                case PrefabPlaceholderEntity placeholder:
                    yield return SpawnChildPlaceholder(prefabPlaceholder, placeholder, childResult);
                    break;
                case WorldEntity worldEntity:
                    yield return SpawnWorldEntityChild(worldEntity, cellRoot, Optional.Of(prefabPlaceholder.gameObject), childResult);
                    break;
                default:
                    Log.Debug(placeholderSlot.ChildEntities.Count > 0 ? $"Unhandled child type {placeholderSlot.ChildEntities[0]}" : "Child was null");
                    break;
            }

            yield return SpawnChildren(slotEntity, childResult.value, cellRoot);
        }
    }

    private IEnumerator SpawnChildPlaceholder(PrefabPlaceholder prefabPlaceholder, PrefabPlaceholderEntity entity, TaskResult<Optional<GameObject>> result)
    {
        IPrefabRequest prefabCoroutine = PrefabDatabase.GetPrefabAsync(prefabPlaceholder.prefabClassId);
        yield return prefabCoroutine;
        prefabCoroutine.TryGetPrefab(out GameObject prefab);
        GameObject gameObject = Utils.SpawnZeroedAt(prefab, prefabPlaceholder.transform, true);

        NitroxEntity.SetNewId(gameObject, entity.Id);

        Optional<EntityMetadataProcessor> metadataProcessor = EntityMetadataProcessor.FromMetaData(entity.Metadata);

        if (metadataProcessor.HasValue)
        {
            metadataProcessor.Value.ProcessMetadata(gameObject, entity.Metadata);
        }

        result.Set(gameObject);
    }

    private IEnumerator SpawnWorldEntityChild(WorldEntity worldEntity, EntityCell cellRoot, Optional<GameObject> parent, TaskResult<Optional<GameObject>> worldEntityResult)
    {
        IWorldEntitySpawner spawner = spawnerResolver.ResolveEntitySpawner(worldEntity);

        yield return spawner.SpawnAsync(worldEntity, parent, cellRoot, worldEntityResult);

        if (worldEntityResult.value.HasValue)
        {
            // For some reasons it's not zero after spawning so we reset it here
            worldEntityResult.value.Value.transform.localPosition = Vector3.zero;
        }
    }

    private IEnumerator SpawnChildren(Entity entity, Optional<GameObject> entityGo, EntityCell cellRoot)
    {
        foreach (Entity child in entity.ChildEntities)
        {
            TaskResult<Optional<GameObject>> childResult = new();

            if (child is PlaceholderGroupWorldEntity placeholderGroup)
            {
                yield return SpawnAsync(placeholderGroup, entityGo, cellRoot, childResult);
            }
            else if(child is WorldEntity worldEntity)
            {
                yield return SpawnWorldEntityChild(worldEntity, cellRoot, entityGo, childResult);
            }

            if (childResult.value.HasValue)
            {
                SpawnChildren(child, childResult.value, cellRoot);
            }
        }
    }

    public bool SpawnsOwnChildren()
    {
        return true;
    }
}
