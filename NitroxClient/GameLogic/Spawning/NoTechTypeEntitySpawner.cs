using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning
{
    /**
     * If we don't have a tech type, then we will just NO-OP the
     * spawn entity function.
     */
    public class NoTechTypeEntitySpawner : IEntitySpawner
    {
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent)
        {
            return Optional<GameObject>.Empty();
        }
    }
}
