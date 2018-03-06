using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    interface IEntityBootstrapper
    {
        void Prepare(Entity spawnedEntity);
    }
}
