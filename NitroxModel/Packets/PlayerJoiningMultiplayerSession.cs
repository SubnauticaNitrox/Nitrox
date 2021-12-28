using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayerJoiningMultiplayerSession : CorrelatedPacket
    {
        [Index(0)]
        public virtual string ReservationKey { get; protected set; }

        public PlayerJoiningMultiplayerSession() : base(default) { }

        public PlayerJoiningMultiplayerSession(string correlationId, string reservationKey) : base(correlationId)
        {
            CorrelationId = correlationId;
            ReservationKey = reservationKey;
        }
    }
}
