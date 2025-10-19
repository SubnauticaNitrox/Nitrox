using Nitrox.Model.DataStructures.GameLogic.Entities;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

public class StayAtLeashPositionBootstrapper : IEntityBootstrapper
{
    public void Prepare(ref WorldEntity spawnedEntity, DeterministicGenerator generator)
    {
        spawnedEntity.Metadata = new StayAtLeashPositionMetadata(spawnedEntity.Transform.Position);
    }
}
