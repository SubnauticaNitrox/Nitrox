using NitroxModel.DataStructures.GameLogic;
using NitroxServer.Helper;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public interface IEntityBootstrapper
    {
        void Prepare(Entity spawnedEntity, Entity parentEntity, DeterministicGenerator idGenerator);
    }
}
