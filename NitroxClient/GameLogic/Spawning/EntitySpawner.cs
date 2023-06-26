using System;
using System.Collections;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

// Implements IEntitySpawner and allows double dispatch to cast to the right type.
public abstract class EntitySpawner<T> : IEntitySpawner where T : Entity
{
    public abstract bool SpawnsOwnChildren(T entity);

    public abstract IEnumerator SpawnAsync(T entity, TaskResult<Optional<GameObject>> result);

    public virtual bool SpawnSync(T entity, TaskResult<Optional<GameObject>> result)
    {
        return false;
    }

    public bool SpawnsOwnChildren(Entity entity)
    {
        return SpawnsOwnChildren((T)entity);
    }

    public IEnumerator SpawnAsync(Entity entity, TaskResult<Optional<GameObject>> result)
    {
        return SpawnAsync((T)entity, result);
    }

    public virtual bool SpawnSync(Entity entity, TaskResult<Optional<GameObject>> result)
    {
        return SpawnSync((T)entity, result);
    }

    /// <returns>The result of <see cref="SpawnSync"/> or true with the caught exception </returns>
    public virtual bool SpawnSyncSafe(Entity entity, TaskResult<Optional<GameObject>> result, TaskResult<Exception> exception)
    {
        try
        {
            if (SpawnSync((T)entity, result))
            {
                exception.Set(null);
                return true;
            }
        } catch (Exception e)
        {
            exception.Set(e);
            return true;
        }
        exception.Set(null);
        return false;
    }
}
