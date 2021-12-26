using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class EscapePodRepair : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }

        private EscapePodRepair() { }

        public EscapePodRepair(NitroxId id)
        {
            Id = id;
        }
    }
}
