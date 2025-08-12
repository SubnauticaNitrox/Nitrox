using System;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Abstract;

public interface ISyncEntitySpawner
{
    bool SpawnSync(Entity entity, TaskResult<Optional<GameObject>> result);

    bool SpawnSyncSafe(Entity entity, TaskResult<Optional<GameObject>> result, TaskResult<Exception> exception);
}
