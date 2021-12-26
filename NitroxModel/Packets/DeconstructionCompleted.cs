using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class DeconstructionCompleted : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }

        private DeconstructionCompleted() { }

        public DeconstructionCompleted(NitroxId id)
        {
            Id = id;
        }
    }
}
