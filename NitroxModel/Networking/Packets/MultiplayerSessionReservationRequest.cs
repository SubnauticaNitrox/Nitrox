using System;
using NitroxModel.Networking.Session;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record MultiplayerSessionReservationRequest : CorrelatedPacket    {
        public PlayerSettings PlayerSettings { get; }
        public AuthenticationContext AuthenticationContext { get; }

        public MultiplayerSessionReservationRequest(string correlationId, PlayerSettings playerSettings, AuthenticationContext authenticationContext) : base(correlationId)
        {
            PlayerSettings = playerSettings;
            AuthenticationContext = authenticationContext;
        }
    }
}
