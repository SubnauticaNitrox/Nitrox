using Nitrox.Model.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

public interface IEntityBootstrapperManager
{
    public void PrepareEntityIfRequired(ref WorldEntity spawnedEntity, DeterministicGenerator generator);
}
