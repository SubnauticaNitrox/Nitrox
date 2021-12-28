using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class SimulationOwnershipRequest : Packet
    {
        [Index(0)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(1)]
        public virtual NitroxId Id { get; protected set; }
        [Index(2)]
        public virtual SimulationLockType LockType { get; protected set; }

        public SimulationOwnershipRequest() { }

        public SimulationOwnershipRequest(ushort playerId, NitroxId id, SimulationLockType lockType)
        {
            PlayerId = playerId;
            Id = id;
            LockType = lockType;
        }
    }
}
