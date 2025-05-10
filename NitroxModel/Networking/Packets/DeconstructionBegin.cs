using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record DeconstructionBegin : Packet
    {
        public NitroxId Id { get; }

        public DeconstructionBegin(NitroxId id)
        {
            Id = id;
        }
    }
}
