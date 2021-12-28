using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public abstract class CorrelatedPacket : Packet
    {
        [Index(-3)]
        public virtual string CorrelationId { get; protected set; }

        public CorrelatedPacket() { }

        protected CorrelatedPacket(string correlationId)
        {
            CorrelationId = correlationId;
        }
    }
}
