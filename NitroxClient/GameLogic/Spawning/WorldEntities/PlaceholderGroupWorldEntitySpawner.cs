using System.Collections;
using System.Linq;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

/// <summary>
/// Group placeholders are unique in that the game cleans up their children each time a player moves out of its batch. In single player, the placeholders
/// would be respawned by iterating over the placeholders - this is patched out in nitrox because we need to tag them. Due to this behavior, we need to 
/// respawn the placeholders when the batch is reloaded.  The top level group entity will still exist in the batch; thus, we will try to load the object
/// by ID first and if we can't find it then spawn the entity in.  
/// </summary>
public class PlaceholderGroupWorldEntitySpawner : IWorldEntitySpawner
{
    // A simple way to detect if the respawn is triggered from a batch deserialization is to tag the group with a mono that is not registered in protobuf.
    // If the group is untagged on a subsequent call then we know to respawn children again. 
    private class NitroxBatchRespawnDetection : MonoBehaviour { };

    private readonly WorldEntitySpawnerResolver spawnerResolver;
    private readonly DefaultWorldEntitySpawner defaultSpawner;

    public PlaceholderGroupWorldEntitySpawner(WorldEntitySpawnerResolver spawnerResolver, DefaultWorldEntitySpawner defaultSpawner)
    {
        this.spawnerResolver = spawnerResolver;
        this.defaultSpawner = defaultSpawner;
    }

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        Optional<GameObject> prefabPlaceholderGroupGameObject = NitroxEntity.GetObjectFrom(entity.Id);

        if (prefabPlaceholderGroupGameObject.HasValue)
        {
            /*
             * Sometimes nested groups are not always parented correctly when deserializing.  This is likely due to UWE's prefab management, where the prefabs are not
             * actually destroyed but deactivated and re-used. In this case, we ensure the proper parenting. 
             */
            if (parent.HasValue)
            {
                prefabPlaceholderGroupGameObject.Value.transform.SetParent(parent.Value.transform, false);
            }

            // If the respawn detection mono is still in place then the entity should still be intact. 
            if (prefabPlaceholderGroupGameObject.Value.GetComponent<NitroxBatchRespawnDetection>())
            {
                result.Set(prefabPlaceholderGroupGameObject);
                yield break;
            }
        }
        else
        {
            TaskResult<Optional<GameObject>> prefabPlaceholderGroupTaskResult = new();
            yield return defaultSpawner.SpawnAsync(entity, parent, cellRoot, prefabPlaceholderGroupTaskResult);
            prefabPlaceholderGroupGameObject = prefabPlaceholderGroupTaskResult.Get();
        }

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

        prefabPlaceholderGroupGameObject.Value.AddComponent<NitroxBatchRespawnDetection>();

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
        TaskResult<GameObject> goResult = new();
        yield return DefaultWorldEntitySpawner.CreateGameObject(TechType.None, prefabPlaceholder.prefabClassId, goResult);

        if (!goResult.value)
        {
            result.Set(Optional.Empty);
        }

        GameObject gameObject = goResult.value;
        gameObject.transform.SetParent(prefabPlaceholder.transform, false);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;

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

            // For some reasons it's not zero after spawning so we reset it here
            worldEntityResult.value.Value.transform.localPosition = Vector3.zero;
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

                    SpawnChildren(child, childResult.value, cellRoot);
                }
            }
        }
    }

    public bool SpawnsOwnChildren()
    {
        return true;
    }
}
