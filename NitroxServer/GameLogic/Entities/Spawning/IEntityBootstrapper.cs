using NitroxServer.Helper;
﻿using NitroxModel.DataStructures.GameLogic.Entities;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public interface IEntityBootstrapper
    {
        void Prepare(WorldEntity spawnedEntity, WorldEntity parentEntity, DeterministicGenerator idGenerator);
    }
}
