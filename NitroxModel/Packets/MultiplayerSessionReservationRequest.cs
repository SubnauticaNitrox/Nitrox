using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MultiplayerSessionReservationRequest : Packet
    {
        public string CorrelationId { get; }
        public string PlayerName { get; }

        public MultiplayerSessionReservationRequest(string correlationId, string playerName)
        {
            CorrelationId = correlationId;
            PlayerName = playerName;
        }
    }
}
