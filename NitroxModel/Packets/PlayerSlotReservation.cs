using NitroxModel.PlayerSlot;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerSlotReservation : Packet
    {
        public PlayerSlotReservationState ReservationState { get; private set; }
        public string CorrelationId { get; private set; }
        public string PlayerId { get; private set; }
        public string ReservationKey { get; private set; }

        public PlayerSlotReservation(string correlationId, PlayerSlotReservationState reservationState)
        {
            CorrelationId = correlationId;
            ReservationState = reservationState;
        }

        public PlayerSlotReservation(string correlationId, string reservationKey, string playerId) : this(correlationId, PlayerSlotReservationState.Reserved)
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
