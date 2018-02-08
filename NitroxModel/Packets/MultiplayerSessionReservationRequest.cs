using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MultiplayerSessionReservationRequest : CorrelatedPacket
    {
        public string PlayerName { get; }

        public MultiplayerSessionReservationRequest(string correlationId, string playerName)
            : base(correlationId)
        {
            PlayerName = playerName;
        }
    }
}
