using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class SimulationOwnershipChange : Packet
    {
        public List<SimulatedEntity> Entities { get; }

        public SimulationOwnershipChange(NitroxId id, ushort owningPlayerId, SimulationLockType lockType, bool changesPosition = false)
        {
            Entities = new List<SimulatedEntity>
            {
                new(id, owningPlayerId, changesPosition, lockType)
            };
        }

        public SimulationOwnershipChange(SimulatedEntity entity)
        {
            Entities = new List<SimulatedEntity>
            {
                entity
            };
        }

        public SimulationOwnershipChange(List<SimulatedEntity> entities)
        {
            Entities = entities;
        }
    }
}
