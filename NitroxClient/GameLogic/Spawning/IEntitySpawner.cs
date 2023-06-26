using System;
using System.Collections;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public interface IEntitySpawner
{
    IEnumerator SpawnAsync(Entity entity, TaskResult<Optional<GameObject>> result);

    bool SpawnSync(Entity entity, TaskResult<Optional<GameObject>> result);

    bool SpawnSyncSafe(Entity entity, TaskResult<Optional<GameObject>> result, TaskResult<Exception> exception);

    bool SpawnsOwnChildren(Entity entity);
}
