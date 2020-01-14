using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipResponse : Packet
    {
        public NitroxId Id { get; }
        public bool LockAquired { get; }
        public SimulationLockType LockType { get; }

        public SimulationOwnershipResponse(NitroxId id, bool lockAquired, SimulationLockType lockType)
        {
            Id = id;
            LockAquired = lockAquired;
            LockType = lockType;
        }

        public override string ToString()
        {
            return "[SimulationOwnershipResponse - Id: " + Id + " LockAquired: " + LockAquired + " LockType: " + LockType + "]";
        }
    }
}
