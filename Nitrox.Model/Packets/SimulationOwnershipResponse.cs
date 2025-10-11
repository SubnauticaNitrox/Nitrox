using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class SimulationOwnershipResponse : Packet
    {
        public NitroxId Id { get; }
        public bool LockAcquired { get; }
        public SimulationLockType LockType { get; }

        public SimulationOwnershipResponse(NitroxId id, bool lockAcquired, SimulationLockType lockType)
        {
            Id = id;
            LockAcquired = lockAcquired;
            LockType = lockType;
        }
    }
}
