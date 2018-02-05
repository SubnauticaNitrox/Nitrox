using NitroxModel.Packets;

namespace NitroxClient.Communication.Abstract
{
    public delegate void MultiplayerSessionManagerStateChangedEventHandler(IMultiplayerSessionManagerState newState);

    public interface IMultiplayerSessionManager : IPacketSender
    {
        IMultiplayerSessionManagerState CurrentState { get; }
        event MultiplayerSessionManagerStateChangedEventHandler StateChanged;

        string IpAddress { get; }
        string Username { get; }
        MultiplaySessionReservation Reservation { get; }
        
        void Connect(string ipAddress);
        void RequestSessionReservation(string userName);
        void ProcessReservationResponsePacket(MultiplaySessionReservation reservation);
        void JoinSession();
        void Disconnect();
    }
}
