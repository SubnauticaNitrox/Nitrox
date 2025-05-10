using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record SimulationOwnershipChange : Packet
    {
        public List<SimulatedEntity> Entities { get; }

        public SimulationOwnershipChange(NitroxId id, PeerId owningPlayerId, SimulationLockType lockType, bool changesPosition = false)
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
