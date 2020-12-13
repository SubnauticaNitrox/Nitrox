using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class DeconstructionBegin : Packet
    {
        public NitroxId Id { get; }

        public DeconstructionBegin(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return "[DeconstructionBegin Id: " + Id + "]";
        }
    }
}
