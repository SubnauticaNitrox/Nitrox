using NitroxModel.DataStructures.GameLogic;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public interface IEntitySpawner
    {
        List<Entity> LoadUnspawnedEntities(Int3 batchId);
    }
}
