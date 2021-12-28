using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class DeconstructionBegin : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }

        public DeconstructionBegin() { }

        public DeconstructionBegin(NitroxId id)
        {
            Id = id;
        }
    }
}
