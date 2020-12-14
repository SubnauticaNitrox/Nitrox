using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerJoiningMultiplayerSession : CorrelatedPacket
    {
        public string ReservationKey { get; }

        public PlayerJoiningMultiplayerSession(string correlationId, string reservationKey) : base(correlationId)
        {
            ReservationKey = reservationKey;
        }

        public override string ToString()
        {
            return $"[PlayerJoiningMultiplayerSession - ReservationKey: {ReservationKey}]";
        }
    }
}
