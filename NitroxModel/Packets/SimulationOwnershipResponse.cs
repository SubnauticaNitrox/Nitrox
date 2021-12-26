using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class SimulationOwnershipResponse : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual bool LockAcquired { get; protected set; }
        [Index(2)]
        public virtual SimulationLockType LockType { get; protected set; }

        private SimulationOwnershipResponse() { }

        public SimulationOwnershipResponse(NitroxId id, bool lockAcquired, SimulationLockType lockType)
        {
            Id = id;
            LockAcquired = lockAcquired;
            LockType = lockType;
        }
    }
}
