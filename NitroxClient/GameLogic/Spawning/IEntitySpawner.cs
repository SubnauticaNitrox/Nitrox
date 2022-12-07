using System.Collections;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning
{
    /**
     * Allows us to create custom entity spawners for different entity types.
     */
    public interface IEntitySpawner
    {
        IEnumerator SpawnAsync(Entity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result);
        bool SpawnsOwnChildren();
    }
}
