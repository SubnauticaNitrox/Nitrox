using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using UnityEngine;

namespace Nitrox.Client.GameLogic.Spawning
{
    /**
     * Allows us to create custom entity spawners for different entity types.
     */
    public interface IEntitySpawner
    {
        Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent, EntityCell cellRoot);
        bool SpawnsOwnChildren();
    }
}
