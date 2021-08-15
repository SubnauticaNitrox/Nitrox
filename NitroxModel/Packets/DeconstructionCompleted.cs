using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionCompleted : Packet
    {
        public NitroxId Id { get; }

        public DeconstructionCompleted(NitroxId id)
        {
            Id = id;
        }
    }
}
