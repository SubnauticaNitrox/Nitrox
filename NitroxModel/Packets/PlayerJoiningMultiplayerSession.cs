using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerJoiningMultiplayerSession : CorrelatedPacket
    {
        public string ReservationKey { get; }

        public PlayerJoiningMultiplayerSession(string correlationId, string reservationKey) : base(correlationId)
        {
            CorrelationId = correlationId;
            ReservationKey = reservationKey;
        }
    }
}
