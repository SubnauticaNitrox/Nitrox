using System;
using Nitrox.Model.MultiplayerSession;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class MultiplayerSessionReservationRequest : CorrelatedPacket
    {
        public PlayerSettings PlayerSettings { get; }
        public AuthenticationContext AuthenticationContext { get; }

        public MultiplayerSessionReservationRequest(string reservationCorrelationId, PlayerSettings playerSettings, AuthenticationContext authenticationContext)
            : base(reservationCorrelationId)
        {
            PlayerSettings = playerSettings;
            AuthenticationContext = authenticationContext;
        }
    }
}
