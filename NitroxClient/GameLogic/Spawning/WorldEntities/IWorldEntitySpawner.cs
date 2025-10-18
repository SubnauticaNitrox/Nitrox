using System.Collections;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities
{
    /**
     * Allows us to create custom entity spawners for different world entity types.
     */
    public interface IWorldEntitySpawner
    {
        IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result);

        bool SpawnsOwnChildren();
    }
}
