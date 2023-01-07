using System.Collections;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class PlayerWorldEntitySpawner : IWorldEntitySpawner
{
    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        // Intial implementation of this processor acts as a no-op.  Eventually, this will replace the need for having a PlayerInitialSync and 
        // RemotePlayerInitialSync.  However, these processes are not yet moved over to the new entity system.  However, we still need a Player
        // entity in the hierarchy to migrate over inventories.  For now, the player is the root object to persist InventoryItemEntities.

        result.Set(Optional.Empty);
        yield break;
    }

    public bool SpawnsOwnChildren()
    {
        return false;
    }
}
