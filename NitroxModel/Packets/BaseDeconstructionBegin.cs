using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class BaseDeconstructionBegin : Packet
    {
        public NitroxId Id { get; }

        public BaseDeconstructionBegin(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return "[BaseDeconstructionBegin Id: " + Id + "]";
        }
    }
}
