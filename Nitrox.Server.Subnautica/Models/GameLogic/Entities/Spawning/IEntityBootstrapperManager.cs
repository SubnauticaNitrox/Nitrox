using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

internal interface IEntityBootstrapperManager
{
    public Task<WorldEntity> PrepareEntityIfRequired(WorldEntity spawnedEntity, DeterministicGenerator generator);
}
