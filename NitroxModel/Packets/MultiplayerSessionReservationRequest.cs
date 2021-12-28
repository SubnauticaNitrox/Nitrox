using NitroxModel.MultiplayerSession;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class MultiplayerSessionReservationRequest : CorrelatedPacket
    {
        [Index(0)]
        public virtual PlayerSettings PlayerSettings { get; protected set; }
        [Index(1)]
        public virtual AuthenticationContext AuthenticationContext { get; protected set; }

        public MultiplayerSessionReservationRequest() : base(default) { }

        public MultiplayerSessionReservationRequest(string reservationCorrelationId, PlayerSettings playerSettings, AuthenticationContext authenticationContext) : base(reservationCorrelationId)
        {
            PlayerSettings = playerSettings;
            AuthenticationContext = authenticationContext;
        }
    }
}
