using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class BaseDeconstructionCompleted : Packet
    {
        public NitroxId Id { get; }

        public BaseDeconstructionCompleted(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return "[DeconstructionCompleted Id: " + Id + "]";
        }
    }
}
