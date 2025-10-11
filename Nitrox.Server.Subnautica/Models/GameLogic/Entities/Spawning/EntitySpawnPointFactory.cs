using System.Collections.Generic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.UnityStubs;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

public abstract class EntitySpawnPointFactory
{
    public abstract List<EntitySpawnPoint> From(AbsoluteEntityCell absoluteEntityCell, NitroxTransform transform, GameObject gameObject);
}
