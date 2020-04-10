using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PrecursorDoorwayToggle : Packet
    {
        public NitroxId Id { get; }
        public bool ToggleDoorway { get; }

        public PrecursorDoorwayToggle(NitroxId id, bool toggleDoorway)
        {
            Id = id;
            ToggleDoorway = toggleDoorway;
        }

        public override string ToString()
        {
            return "[PrecursorDoorway id: " + Id + ", toggleDoorway: " + ToggleDoorway + "]";
        }
    }
}
