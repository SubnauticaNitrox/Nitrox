using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class EscapePodRadioRepair : Packet
    {
        public NitroxId Id { get; }

        public EscapePodRadioRepair(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return "[EscapePodRadioRepair id: " + Id + "]";
        }
    }
}
