using System.Collections.Generic;
using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class SimulationOwnershipChange : Packet
    {
        [Index(0)]
        public virtual List<SimulatedEntity> Entities { get; protected set; }

        private SimulationOwnershipChange() { }

        public SimulationOwnershipChange(NitroxId id, ushort owningPlayerId, SimulationLockType lockType)
        {
            Entities = new List<SimulatedEntity>
            {
                new(id, owningPlayerId, false, lockType)
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
