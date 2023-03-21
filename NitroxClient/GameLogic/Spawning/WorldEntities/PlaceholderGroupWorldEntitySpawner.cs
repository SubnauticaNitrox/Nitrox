using System.Collections;
using System.Linq;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

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

        // Spawning PrefabPlaceholders as siblings to the group
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
                    yield return SpawnWorldEntityChild(worldEntity, cellRoot, Optional.Of(prefabPlaceholder.transform.parent.gameObject), childResult);
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
        TaskResult<GameObject> goResult = new();
        yield return DefaultWorldEntitySpawner.CreateGameObject(TechType.None, prefabPlaceholder.prefabClassId, goResult);

        if (!goResult.value)
        {
            result.Set(Optional.Empty);
        }

        GameObject gameObject = goResult.value;
        gameObject.transform.SetParent(prefabPlaceholder.transform.parent, false);
        gameObject.transform.localPosition = prefabPlaceholder.transform.localPosition;
        gameObject.transform.localRotation = prefabPlaceholder.transform.localRotation;

        NitroxEntity.SetNewId(gameObject, entity.Id);

        EntityMetadataProcessor.ApplyMetadata(gameObject, entity.Metadata);

        result.Set(gameObject);
    }

    private IEnumerator SpawnWorldEntityChild(WorldEntity worldEntity, EntityCell cellRoot, Optional<GameObject> parent, TaskResult<Optional<GameObject>> worldEntityResult)
    {
        IWorldEntitySpawner spawner = spawnerResolver.ResolveEntitySpawner(worldEntity);

        yield return spawner.SpawnAsync(worldEntity, parent, cellRoot, worldEntityResult);

        if (worldEntityResult.value.HasValue)
        {
            EntityMetadataProcessor.ApplyMetadata(worldEntityResult.value.Value, worldEntity.Metadata);

            worldEntityResult.value.Value.transform.localPosition = worldEntity.Transform.LocalPosition.ToUnity();
            worldEntityResult.value.Value.transform.localRotation = worldEntity.Transform.LocalRotation.ToUnity();
        }
    }

    private IEnumerator SpawnChildren(Entity entity, Optional<GameObject> entityGo, EntityCell cellRoot)
    {
        foreach (Entity child in entity.ChildEntities)
        {
            if(child is WorldEntity worldEntity)
            {
                TaskResult<Optional<GameObject>> childResult = new();

                yield return SpawnWorldEntityChild(worldEntity, cellRoot, entityGo, childResult);

                if (childResult.value.HasValue)
                {
                    EntityMetadataProcessor.ApplyMetadata(childResult.value.Value, child.Metadata);

                    yield return SpawnChildren(child, childResult.value, cellRoot);
                }
            }
        }
    }

    public bool SpawnsOwnChildren()
    {
        return true;
    }
}
