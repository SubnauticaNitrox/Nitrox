using System;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MultiplayerSessionReservationRequest : CorrelatedPacket
    {
        public PlayerSettings PlayerSettings { get; }
        public AuthenticationContext AuthenticationContext { get; }

        public MultiplayerSessionReservationRequest(string correlationId, PlayerSettings playerSettings, AuthenticationContext authenticationContext) : base(correlationId)
        {
            PlayerSettings = playerSettings;
            AuthenticationContext = authenticationContext;
        }
    }
}
