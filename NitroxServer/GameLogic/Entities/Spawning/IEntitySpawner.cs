using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public interface IEntitySpawner
    {
        List<Entity> LoadUnspawnedEntities(NitroxInt3 batchId, bool fullCacheCreation = false);
    }
}
