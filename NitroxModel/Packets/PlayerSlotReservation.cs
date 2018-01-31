using NitroxModel.PlayerSlot;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerSlotReservation : Packet
    {
        public PlayerSlotReservationState ReservationState { get; set; }
        public string CorrelationId { get; set; }
        public string PlayerId { get; set; }
        public string ReservationKey { get; set; }

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
