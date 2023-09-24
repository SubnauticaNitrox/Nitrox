using System.Collections;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Abstract;

public interface IEntitySpawner
{
    IEnumerator SpawnAsync(Entity entity, TaskResult<Optional<GameObject>> result);

    bool SpawnsOwnChildren(Entity entity);
}
