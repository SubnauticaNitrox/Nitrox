using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipResponse : Packet
    {
        public string Guid { get; }
        public bool LockAquired { get; }

        public SimulationOwnershipResponse(string guid, bool lockAquired)
        {
            Guid = guid;
            LockAquired = lockAquired;
        }

        public override string ToString()
        {
            return "[SimulationOwnershipResponse - Guid: " + Guid + " LockAquired: " + LockAquired + "]";
        }
    }
}
