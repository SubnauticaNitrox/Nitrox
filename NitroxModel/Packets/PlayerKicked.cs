using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayerKicked : Packet
    {
        [Index(0)]
        public virtual string Reason { get; protected set; }

        public PlayerKicked() { }

        public PlayerKicked(string reason)
        {
            Reason = reason;
        }
    }
}
