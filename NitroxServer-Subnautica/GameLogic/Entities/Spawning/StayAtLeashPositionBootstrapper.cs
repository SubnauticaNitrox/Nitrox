using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Helper;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning;

public class StayAtLeashPositionBootstrapper : IEntityBootstrapper
{
    public void Prepare(ref WorldEntity spawnedEntity, DeterministicGenerator generator)
    {
        spawnedEntity.Metadata = new StayAtLeashPositionMetadata(spawnedEntity.Transform.Position);
    }
}
