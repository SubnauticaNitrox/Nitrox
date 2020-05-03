using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public interface IEntitySpawner
    {
        List<Entity> LoadUnspawnedEntities(NitroxModel.DataStructures.Int3 batchId);
    }
}
