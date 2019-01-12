using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Entities.Spawning.EntityBootstrappers
{
    interface IEntityBootstrapper
    {
        void Prepare(Entity spawnedEntity, DeterministicBatchGenerator guidGenerator);
    }
}
