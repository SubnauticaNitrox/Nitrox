using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public interface IWorldEntitySyncSpawner
{
    bool SpawnSync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result);
}
