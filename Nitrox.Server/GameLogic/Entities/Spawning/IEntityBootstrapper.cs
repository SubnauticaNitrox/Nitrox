using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Server.GameLogic.Entities.Spawning
{
    public interface IEntityBootstrapper
    {
        void Prepare(Entity spawnedEntity, Entity parentEntity, DeterministicBatchGenerator idGenerator);
    }
}
