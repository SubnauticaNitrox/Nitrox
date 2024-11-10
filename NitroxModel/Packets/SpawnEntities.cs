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

        public SpawnEntities(Entity entity, SimulatedEntity simulatedEntity = null, bool forceRespawn = false)
        {
            Entities = [entity];
            Simulations = [];
            if (simulatedEntity != null)
            {
                Simulations.Add(simulatedEntity);
            }

            ForceRespawn = forceRespawn;
        }

        // Constructor for serialization. 
        public SpawnEntities(List<Entity> entities, List<SimulatedEntity> simulations, bool forceRespawn)
        {
            Entities = entities;
            Simulations = simulations;
            ForceRespawn = forceRespawn;
        }
    }
}
