using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

internal sealed class StayAtLeashPositionBootstrapper : IEntityBootstrapper
{
    public void Prepare(ref WorldEntity spawnedEntity, DeterministicGenerator generator)
    {
        spawnedEntity.Metadata = new StayAtLeashPositionMetadata(spawnedEntity.Transform.Position);
    }
}
