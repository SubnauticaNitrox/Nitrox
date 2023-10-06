using System;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Abstract;

public abstract class SyncEntitySpawner<T> : EntitySpawner<T>, ISyncEntitySpawner where T : Entity
{
    protected abstract bool SpawnSync(T entity, TaskResult<Optional<GameObject>> result);

    public bool SpawnSync(Entity entity, TaskResult<Optional<GameObject>> result)
    {
        return SpawnSync((T)entity, result);
    }

    /// <returns>The result of <see cref="SpawnSync(T,TaskResult{Optional{GameObject}})"/> or true with the caught exception </returns>
    public bool SpawnSyncSafe(Entity entity, TaskResult<Optional<GameObject>> result, TaskResult<Exception> exception)
    {
        try
        {
            if (SpawnSync((T)entity, result))
            {
                exception.Set(null);
                return true;
            }
        }
        catch (Exception e)
        {
            exception.Set(e);
            return true;
        }
        exception.Set(null);
        return false;
    }
}
