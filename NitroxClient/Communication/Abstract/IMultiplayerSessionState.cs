using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Abstract
{
    public interface IMultiplayerSessionState
    {
        IClient Client { get; }
        string IpAddress { get; }
        int ServerPort { get; }
        MultiplayerSessionPolicy SessionPolicy { get; }
        PlayerSettings PlayerSettings { get; }
        AuthenticationContext AuthenticationContext { get; }
        MultiplayerSessionReservation Reservation { get; }
    }
}
