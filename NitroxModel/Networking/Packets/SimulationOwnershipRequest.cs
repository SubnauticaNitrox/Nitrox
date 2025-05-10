using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record SimulationOwnershipRequest : Packet
    {
        public ushort PlayerId { get; }
        public NitroxId Id { get; }
        public SimulationLockType LockType { get; }

        public SimulationOwnershipRequest(ushort playerId, NitroxId id, SimulationLockType lockType)
        {
            PlayerId = playerId;
            Id = id;
            LockType = lockType;
        }
    }
}
