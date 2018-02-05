using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class JoinMultiplayerSessionRequest : Packet
    {
        public string CorrelationId { get; }
        public string ReservationKey { get; }

        public JoinMultiplayerSessionRequest(string correlationId, string reservationKey)
        {
            CorrelationId = correlationId;
            ReservationKey = reservationKey;
        }
    }
}
