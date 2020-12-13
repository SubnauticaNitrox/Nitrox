using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class EscapePodRepair : Packet
    {
        public NitroxId Id { get; }

        public EscapePodRepair(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return "[EscapePodRepair id: " + Id + "]";
        }
    }
}
