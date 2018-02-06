using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerJoiningMultiplayerSession : Packet
    {
        public string CorrelationId { get; }
        public string ReservationKey { get; }

        public PlayerJoiningMultiplayerSession(string correlationId, string reservationKey)
        {
            CorrelationId = correlationId;
            ReservationKey = reservationKey;
        }
    }
}
