using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class Disconnect : Packet
    {
        [Index(0)]
        public virtual ushort PlayerId { get; protected set; }

        private Disconnect() { }

        public Disconnect(ushort playerId)
        {
            PlayerId = playerId;
        }
    }
}
