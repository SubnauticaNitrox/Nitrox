using System.Collections;
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

        for (int index = 0; index < placeholderGroupEntity.ChildEntities.Count; index++)
        {
            Entity placeholderSlot = placeholderGroupEntity.ChildEntities[index];

            if (placeholderSlot.ChildEntities.Count == 0) //Entity was a slot not spawned, picked up, or removed
            {
                continue;
            }

            PrefabPlaceholder prefabPlaceholder = prefabPlaceholderGroup.prefabPlaceholders[index];

            switch (placeholderSlot.ChildEntities[0])
            {
                case PrefabPlaceholderEntity placeholder:
                    IPrefabRequest prefabCoroutine = PrefabDatabase.GetPrefabAsync(prefabPlaceholder.prefabClassId);
                    yield return prefabCoroutine;
                    prefabCoroutine.TryGetPrefab(out GameObject prefab);
                    GameObject gameObject = Utils.SpawnZeroedAt(prefab, prefabPlaceholder.transform, true);

                    NitroxEntity.SetNewId(gameObject, placeholder.Id);

                    Optional<EntityMetadataProcessor> metadataProcessor = EntityMetadataProcessor.FromMetaData(entity.Metadata);

                    if (metadataProcessor.HasValue)
                    {
                        metadataProcessor.Value.ProcessMetadata(gameObject, placeholder.Metadata);
                    }

                    break;
                case WorldEntity slotEntity:
                    IWorldEntitySpawner spawner = spawnerResolver.ResolveEntitySpawner(slotEntity);
                    Optional<GameObject> slotEntityParent = Optional.Of(prefabPlaceholder.gameObject);

                    TaskResult<Optional<GameObject>> slotEntityTaskResult = new();
                    yield return spawner.SpawnAsync(slotEntity, slotEntityParent, cellRoot, slotEntityTaskResult);

                    if (slotEntityTaskResult.value.HasValue)
                    {
                        // For some reasons it's not zero after spawning so we reset it here
                        slotEntityTaskResult.value.Value.transform.localPosition = Vector3.zero;
                    }

                    break;
                default:
                    Log.Debug(placeholderSlot.ChildEntities.Count > 0 ? $"Unhandled child type {placeholderSlot.ChildEntities[0]}" : "Child was null");
                    break;
            }
        }
    }

    public bool SpawnsOwnChildren()
    {
        return true;
    }
}
