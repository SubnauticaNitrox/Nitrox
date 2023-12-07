using System.Collections;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class PrefabPlaceholderEntitySpawner : IWorldEntitySpawner, IWorldEntitySyncSpawner
{
    private readonly DefaultWorldEntitySpawner defaultEntitySpawner;

    public PrefabPlaceholderEntitySpawner(DefaultWorldEntitySpawner defaultEntitySpawner)
    {
        this.defaultEntitySpawner = defaultEntitySpawner;
    }

    // TODO: Clean the spawners (move to to Setup() and IsValidOrError() after rebase)
    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not PrefabPlaceholderEntity prefabEntity)
        {
            yield break;
        }
        if (!parent.Value || !parent.Value.TryGetComponent(out PrefabPlaceholdersGroup group))
        {
            Log.Error($"[{nameof(PrefabPlaceholderEntity)}] Can't find a {nameof(PrefabPlaceholdersGroup)} on parent for {entity.Id}");
            yield break;
        }
        PrefabPlaceholder placeholder = group.prefabPlaceholders[prefabEntity.ComponentIndex];
        yield return defaultEntitySpawner.SpawnAsync(entity, placeholder.transform.parent.gameObject, cellRoot, result);
        if (!result.value.HasValue)
        {
            yield break;
        }
        SetupObject(entity, result.value.Value);
    }

    public bool SpawnsOwnChildren() => false;

    public bool SpawnSync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not PrefabPlaceholderEntity prefabEntity)
        {
            return true;
        }
        if (!parent.Value || !parent.Value.TryGetComponent(out PrefabPlaceholdersGroup group))
        {
            Log.Error($"[{nameof(PrefabPlaceholderEntity)}] Can't find a {nameof(PrefabPlaceholdersGroup)} on parent for {entity.Id}");
            return true;
        }
        PrefabPlaceholder placeholder = group.prefabPlaceholders[prefabEntity.ComponentIndex];
        if (!defaultEntitySpawner.SpawnSync(entity, placeholder.transform.parent.gameObject, cellRoot, result))
        {
            return false;
        }
        SetupObject(entity, result.value.Value);
        return true;
    }

    private void SetupObject(WorldEntity entity, GameObject gameObject)
    {
        gameObject.transform.localPosition = entity.Transform.LocalPosition.ToUnity();
        gameObject.transform.localRotation = entity.Transform.LocalRotation.ToUnity();
        gameObject.transform.localScale = entity.Transform.LocalScale.ToUnity();
    }
}
