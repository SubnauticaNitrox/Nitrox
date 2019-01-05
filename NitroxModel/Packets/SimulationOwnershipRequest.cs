using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipRequest : Packet
    {
        public ushort PlayerId { get; }
        public string Guid { get; }
        public SimulationLockType LockType { get; }

        public SimulationOwnershipRequest(ushort playerId, string guid, SimulationLockType lockType)
        {
            PlayerId = playerId;
            Guid = guid;
            LockType = lockType;
        }

        public override string ToString()
        {
            return "[SimulationOwnershipRequest - PlayerId: " + PlayerId + " Guid: " + Guid + " PlayerId: " + PlayerId + " LockType: " + LockType + "]";
        }
    }
}
