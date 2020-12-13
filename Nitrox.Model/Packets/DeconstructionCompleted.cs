using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class DeconstructionCompleted : Packet
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
