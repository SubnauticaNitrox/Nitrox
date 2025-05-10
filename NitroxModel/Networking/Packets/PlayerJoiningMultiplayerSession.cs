using System;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record PlayerJoiningMultiplayerSession : CorrelatedPacket    {
        public string ReservationKey { get; }

        public PlayerJoiningMultiplayerSession(string correlationId, string reservationKey) : base(correlationId)
        {
            CorrelationId = correlationId;
            ReservationKey = reservationKey;
        }
    }
}
