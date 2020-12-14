using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipChange : Packet
    {
        public List<SimulatedEntity> Entities { get; }

        public SimulationOwnershipChange(NitroxId id, ushort owningPlayerId, SimulationLockType lockType)
        {
            Entities = new List<SimulatedEntity> { new SimulatedEntity(id, owningPlayerId, false, lockType) };
        }

        public SimulationOwnershipChange(SimulatedEntity entity)
        {
            Entities = new List<SimulatedEntity> { entity };
        }

        public SimulationOwnershipChange(List<SimulatedEntity> entities)
        {
            Entities = entities;
        }

        public override string ToString()
        {
            return $"[SimulationOwnershipChange - Entities: {Entities?.Count}]";
        }

        public override string ToLongString()
        {
            return $"[SimulationOwnershipChange - Entities: ({string.Join(", ", Entities)})]";
        }
    }
}
