using System;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MultiplayerSessionReservation : CorrelatedPacket
    {
        public MultiplayerSessionReservationState ReservationState { get; }
        public ushort PlayerId { get; }
        public string ReservationKey { get; }

        public MultiplayerSessionReservation(string correlationId, MultiplayerSessionReservationState reservationState)
            : base(correlationId)
        {
            ReservationState = reservationState;
        }

        public MultiplayerSessionReservation(string correlationId, ushort playerId, string reservationKey)
            : this(correlationId, MultiplayerSessionReservationState.RESERVED)
        {
            PlayerId = playerId;
            ReservationKey = reservationKey;
        }

        public override string ToString()
        {
            return $"ReservationState: {ReservationState.ToString()} - ReservationKey: {ReservationKey}";
        }
    }
}
