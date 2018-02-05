using System;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MultiplaySessionReservation : Packet
    {
        public MultiplayerSessionReservationState ReservationState { get; }
        public string CorrelationId { get; }
        public string PlayerId { get; }
        public string ReservationKey { get; }

        public MultiplaySessionReservation(string correlationId, MultiplayerSessionReservationState reservationState)
        {
            CorrelationId = correlationId;
            ReservationState = reservationState;
        }

        public MultiplaySessionReservation(string correlationId, string reservationKey, string playerId) : this(correlationId, MultiplayerSessionReservationState.Reserved)
        {
            PlayerId = playerId;
            ReservationKey = reservationKey;
        }

        public override string ToString()
        {
            return $"PlayerId: {PlayerId} - ReservationState: { ReservationState.ToString() } - ReservationKey: { ReservationKey }";
        }
    }
}
