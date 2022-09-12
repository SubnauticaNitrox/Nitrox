using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionCompleted : Packet
    {
        public NitroxId Id { get; }
        public bool Absolute { get; }

        public DeconstructionCompleted(NitroxId id, bool absolute = false)
        {
            Id = id;
            Absolute = absolute;
        }
    }
}
