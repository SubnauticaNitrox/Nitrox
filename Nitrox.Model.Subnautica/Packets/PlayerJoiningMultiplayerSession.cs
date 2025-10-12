using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
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
