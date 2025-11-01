using System.Collections;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Abstract;

public interface IEntitySpawner
{
    IEnumerator SpawnAsync(Entity entity, TaskResult<Optional<GameObject>> result);

    bool SpawnsOwnChildren(Entity entity);
}
