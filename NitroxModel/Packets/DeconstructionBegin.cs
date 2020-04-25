using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets.Core;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionBegin : Packet, IVolatilePacket
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
