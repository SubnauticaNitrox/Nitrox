using System.Threading.Tasks;
using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Session;

namespace NitroxClient.Communication.Abstract
{
    public delegate void MultiplayerSessionConnectionStateChangedEventHandler(IMultiplayerSessionConnectionState newState);

    public interface IMultiplayerSession : IPacketSender, IMultiplayerSessionState
    {
        IMultiplayerSessionConnectionState CurrentState { get; }
        event MultiplayerSessionConnectionStateChangedEventHandler ConnectionStateChanged;

        Task ConnectAsync(string ipAddress, int port);
        void ProcessSessionPolicy(MultiplayerSessionPolicy policy);
        void RequestSessionReservation(PlayerSettings playerSettings, AuthenticationContext authenticationContext);
        void ProcessReservationResponsePacket(MultiplayerSessionReservation reservation);
        void JoinSession();
        void Disconnect();
    }
}
