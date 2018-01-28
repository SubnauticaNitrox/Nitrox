using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ClaimPlayerSlotReservation : Packet
    {
        public string CorrelationId { get; set; }
        public string ReservationKey { get; set; }

        public ClaimPlayerSlotReservation(string correlationId, string reservationKey)
        {
            CorrelationId = correlationId;
            ReservationKey = reservationKey;
        }
    }
}
