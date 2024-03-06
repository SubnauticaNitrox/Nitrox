using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxServer.Helper;

namespace NitroxServer.GameLogic.Entities.Spawning;

public interface IEntityBootstrapper
{
    public void Prepare(ref WorldEntity spawnedEntity, DeterministicGenerator generator);
}

