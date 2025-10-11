using Nitrox.Model.DataStructures.GameLogic.Entities;
using Nitrox.Model.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public interface IWorldEntitySyncSpawner
{
    bool SpawnSync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result);
}
