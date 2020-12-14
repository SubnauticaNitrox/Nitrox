using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
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
            return $"[DeconstructionBegin - Id: {Id}]";
        }
    }
}
