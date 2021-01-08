using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Abstract
{
    public interface IMultiplayerSessionState
    {
        IClient Client { get; }
        IConnectionInfo ConnectionInfo { get; }
        MultiplayerSessionPolicy SessionPolicy { get; }
        PlayerSettings PlayerSettings { get; }
        AuthenticationContext AuthenticationContext { get; }
        MultiplayerSessionReservation Reservation { get; }
    }
}
