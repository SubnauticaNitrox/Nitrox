using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.UnityStubs;

namespace Nitrox.Server.GameLogic.Entities.Spawning
{
    public abstract class EntitySpawnPointFactory
    {
        public abstract List<EntitySpawnPoint> From(AbsoluteEntityCell absoluteEntityCell, NitroxTransform transform, GameObject gameObject);
    }
}
