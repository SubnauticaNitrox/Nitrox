using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PrecursorDoorwayAction : Packet
    {
        public NitroxId Id { get; }
        public bool isOpen { get; }

        public PrecursorDoorwayAction(NitroxId id, bool isOpen)
        {
            Id = id;
            this.isOpen = isOpen;
        }

        public override string ToString()
        {
            return "[PrecursorDoorway id: " + Id + ", isOpen: " + isOpen + "]";
        }
    }
}
