
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.Serialization.Resources.Datastructures;
using NitroxServer.UnityStubs;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public abstract class EntitySpawnPointFactory
    {
        public abstract List<EntitySpawnPoint> From(AbsoluteEntityCell absoluteEntityCell, TransformAsset transform, GameObject gameObject);
    }
}
