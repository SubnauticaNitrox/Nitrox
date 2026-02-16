using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

internal interface IEntityBootstrapperManager
{
    public void PrepareEntityIfRequired(ref WorldEntity spawnedEntity, DeterministicGenerator generator);
}
