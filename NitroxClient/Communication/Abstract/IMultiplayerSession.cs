using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Networking;

namespace NitroxClient.Communication.Abstract
{
    public delegate void MultiplayerSessionConnectionStateChangedEventHandler(IMultiplayerSessionConnectionState newState);

    public interface IMultiplayerSession : IPacketSender, IMultiplayerSessionState
    {
        IMultiplayerSessionConnectionState CurrentState { get; }
        event MultiplayerSessionConnectionStateChangedEventHandler ConnectionStateChanged;

        void Connect(IConnectionInfo connectionInfo);
        void ProcessSessionPolicy(MultiplayerSessionPolicy policy);
        void RequestSessionReservation(PlayerSettings playerSettings, AuthenticationContext authenticationContext);
        void ProcessReservationResponsePacket(MultiplayerSessionReservation reservation);
        void JoinSession();
        void Disconnect();
    }
}
