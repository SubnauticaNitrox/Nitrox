using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ClaimPlayerSlotReservation : Packet
    {
        public string CorrelationId { get; private set; }
        public string ReservationKey { get; private set; }

        public ClaimPlayerSlotReservation(string correlationId, string reservationKey)
        {
            CorrelationId = correlationId;
            ReservationKey = reservationKey;
        }
    }
}
