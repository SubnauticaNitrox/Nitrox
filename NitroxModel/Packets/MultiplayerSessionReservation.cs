using System;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MultiplayerSessionReservation : Packet
    {
        public MultiplayerSessionReservationState ReservationState { get; }
        public string CorrelationId { get; }
        public string PlayerId { get; }
        public string ReservationKey { get; }

        public MultiplayerSessionReservation(string correlationId, MultiplayerSessionReservationState reservationState)
        {
            CorrelationId = correlationId;
            ReservationState = reservationState;
        }

        public MultiplayerSessionReservation(string correlationId, string reservationKey, string playerId) : this(correlationId, MultiplayerSessionReservationState.Reserved)
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
