using NitroxModel.MultiplayerSession;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class MultiplayerSessionReservation : CorrelatedPacket
    {
        [Index(0)]
        public virtual MultiplayerSessionReservationState ReservationState { get; protected set; }
        [Index(1)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(2)]
        public virtual string ReservationKey { get; protected set; }

        public MultiplayerSessionReservation() : base(default) { }

        public MultiplayerSessionReservation(string correlationId, MultiplayerSessionReservationState reservationState) : base(correlationId)
        {
            ReservationState = reservationState;
        }

        public MultiplayerSessionReservation(string correlationId, ushort playerId, string reservationKey) : this(correlationId, MultiplayerSessionReservationState.RESERVED)
        {
            PlayerId = playerId;
            ReservationKey = reservationKey;
        }
    }
}
