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
    }
}
