using NitroxModel.PlayerSlot;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerSlotReservation : Packet
    {
        public ReservationRejectionReason ReservationRejectionReason { get; set; }
        public string CorrelationId { get; set; }
        public string PlayerId { get; set; }
        public string ReservationKey { get; set; }

        public PlayerSlotReservation(string correlationId, ReservationRejectionReason rejectionReason)
        {
            CorrelationId = correlationId;
            ReservationRejectionReason = rejectionReason;
        }

        public PlayerSlotReservation(string correlationId, string reservationKey, string playerId) : this(correlationId, ReservationRejectionReason.None)
        {
            PlayerId = playerId;
            ReservationKey = reservationKey;
        }
    }
}
