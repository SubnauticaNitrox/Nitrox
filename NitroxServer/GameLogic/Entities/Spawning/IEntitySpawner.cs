using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public interface IEntitySpawner
    {
        List<Entity> LoadUnspawnedEntities(Int3 batchId);
    }
}
