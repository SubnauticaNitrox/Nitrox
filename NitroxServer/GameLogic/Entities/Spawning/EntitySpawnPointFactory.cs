using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using NitroxServer.UnityStubs;

namespace NitroxServer.GameLogic.Entities.Spawning;

public abstract class EntitySpawnPointFactory
{
    public abstract List<EntitySpawnPoint> From(AbsoluteEntityCell absoluteEntityCell, NitroxTransform transform, GameObject gameObject);
}
