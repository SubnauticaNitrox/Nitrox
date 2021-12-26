using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class EscapePodRadioRepair : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }

        private EscapePodRadioRepair() { }

        public EscapePodRadioRepair(NitroxId id)
        {
            Id = id;
        }
    }
}
