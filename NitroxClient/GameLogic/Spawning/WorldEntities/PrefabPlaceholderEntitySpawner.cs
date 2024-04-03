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

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (!VerifyCanSpawnOrError(entity, parent, out PrefabPlaceholder placeholder))
        {
            yield break;
        }

        yield return defaultEntitySpawner.SpawnAsync(entity, placeholder.transform.parent.gameObject, cellRoot, result);        
        if (!result.value.HasValue)
        {
            yield break;
        }

        SetupObject(entity, result.value.Value, placeholder.transform.parent.gameObject);
    }

    public bool SpawnsOwnChildren() => false;

    public bool SpawnSync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (!VerifyCanSpawnOrError(entity, parent, out PrefabPlaceholder placeholder))
        {
            return true;
        }

        if (!defaultEntitySpawner.SpawnSync(entity, placeholder.transform.parent.gameObject, cellRoot, result))
        {
            return false;
        }
        
        SetupObject(entity, result.value.Value, placeholder.transform.parent.gameObject);
        return true;
    }

    private bool VerifyCanSpawnOrError(WorldEntity entity, Optional<GameObject> parent, out PrefabPlaceholder placeholder)
    {
        if (entity is PrefabPlaceholderEntity prefabEntity &&
            parent.Value && parent.Value.TryGetComponent(out PrefabPlaceholdersGroup group))
        {
            placeholder = group.prefabPlaceholders[prefabEntity.ComponentIndex];
            return true;
        }

        Log.Error($"[{nameof(PrefabPlaceholderEntitySpawner)}] Can't find a {nameof(PrefabPlaceholdersGroup)} on parent for {entity.Id}");
        placeholder = null;
        return false;
    }

    private void SetupObject(WorldEntity entity, GameObject gameObject, GameObject parent)
    {
        if (entity is PrefabPlaceholderEntity prefabEntity && !prefabEntity.IsEntitySlotEntity && parent)
        {
            gameObject.transform.localPosition = entity.Transform.LocalPosition.ToUnity();
            gameObject.transform.localRotation = entity.Transform.LocalRotation.ToUnity();
            gameObject.transform.localScale = entity.Transform.LocalScale.ToUnity();
            gameObject.transform.SetParent(parent.transform, false);
        }
    }
}
