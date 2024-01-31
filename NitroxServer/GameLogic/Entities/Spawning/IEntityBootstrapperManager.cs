using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxServer.Helper;

namespace NitroxServer.GameLogic.Entities.Spawning;

public interface IEntityBootstrapperManager
{
    public void PrepareEntityIfRequired(ref WorldEntity spawnedEntity, DeterministicGenerator generator);
}
