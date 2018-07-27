using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipResponse : Packet
    {
        public string Guid { get; }
        public bool LockAquired { get; }
        public SimulationLockType LockType { get; }

        public SimulationOwnershipResponse(string guid, bool lockAquired, SimulationLockType lockType)
        {
            Guid = guid;
            LockAquired = lockAquired;
            LockType = lockType;
        }

        public override string ToString()
        {
            return "[SimulationOwnershipResponse - Guid: " + Guid + " LockAquired: " + LockAquired + " LockType: " + LockType + "]";
        }
    }
}
