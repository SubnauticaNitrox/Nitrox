using System.Collections;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Abstract;

// Implements IEntitySpawner and allows double dispatch to cast to the right type.
public abstract class EntitySpawner<T> : IEntitySpawner where T : Entity
{
    protected abstract bool SpawnsOwnChildren(T entity);

    protected abstract IEnumerator SpawnAsync(T entity, TaskResult<Optional<GameObject>> result);

    public bool SpawnsOwnChildren(Entity entity)
    {
        return SpawnsOwnChildren((T)entity);
    }

    public IEnumerator SpawnAsync(Entity entity, TaskResult<Optional<GameObject>> result)
    {
        return SpawnAsync((T)entity, result);
    }
}
