using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning
{
    public interface IEntitySpawner
    {
        List<Entity> LoadUnspawnedEntities(NitroxInt3 batchId, bool fullCacheCreation = false);
    }
}
