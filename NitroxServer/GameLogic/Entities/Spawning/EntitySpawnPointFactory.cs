using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxServer.Serialization.Resources.UnityStubs;

namespace NitroxServer.GameLogic.Entities.Spawning;

public abstract class EntitySpawnPointFactory
{
    public abstract List<EntitySpawnPoint> From(AbsoluteEntityCell absoluteEntityCell, NitroxTransform transform, GameObject gameObject);
}
