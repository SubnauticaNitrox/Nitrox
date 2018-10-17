using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipRequest : Packet
    {
        public ulong LPlayerId { get; }
        public string Guid { get; }
        public SimulationLockType LockType { get; }

        public SimulationOwnershipRequest(ulong playerId, string guid, SimulationLockType lockType)
        {
            LPlayerId = playerId;
            Guid = guid;
            LockType = lockType;
        }

        public override string ToString()
        {
            return "[SimulationOwnershipRequest - PlayerId: " + LPlayerId + " Guid: " + Guid + " PlayerId: " + LPlayerId + " LockType: " + LockType + "]";
        }
    }
}
