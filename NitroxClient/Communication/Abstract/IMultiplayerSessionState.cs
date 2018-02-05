using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Abstract
{
    public interface IMultiplayerSessionState
    {
        string IpAddress { get; }
        MultiplayerSessionPolicy SessionPolicy { get; }
        PlayerSettings PlayerSettings { get; }
        AuthenticationContext Authentication { get; }
        MultiplayerSessionReservation Reservation { get; }
    }
}