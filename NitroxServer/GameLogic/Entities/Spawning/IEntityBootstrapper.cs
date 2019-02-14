using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic.Entities.Spawning;

namespace NitroxServer.GameLogic.Entities.EntityBootstrappers
{
    public interface IEntityBootstrapper
    {
        void Prepare(Entity spawnedEntity, DeterministicBatchGenerator guidGenerator);
    }
}
