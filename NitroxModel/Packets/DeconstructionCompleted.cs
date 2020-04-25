using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets.Core;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionCompleted : Packet, IVolatilePacket
    {
        public NitroxId Id { get; }

        public DeconstructionCompleted(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return "[DeconstructionCompleted Id: " + Id + "]";
        }
    }
}
