using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SpawnEntities : Packet
    {
        public List<Entity> Entities { get; }
        public List<SimulatedEntity> Simulations { get; }

        public bool ForceRespawn { get; }

        public SpawnEntities(List<Entity> entities)
        {
            Entities = entities;
            ForceRespawn = false;
        }

        public SpawnEntities(Entity entity, bool forceRespawn = false, SimulatedEntity simulatedEntity = null)
        {
            Entities = [entity];
            Simulations = [simulatedEntity];

            ForceRespawn = forceRespawn;
        }

        // Constructor for serialization. 
        public SpawnEntities(List<Entity> entities, bool forceRespawn, List<SimulatedEntity> simulations)
        {
            Entities = entities;
            ForceRespawn = forceRespawn;
            Simulations = simulations;
        }
    }
}
