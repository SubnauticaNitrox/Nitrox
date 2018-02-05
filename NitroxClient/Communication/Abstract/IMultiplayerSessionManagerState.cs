using NitroxModel.Packets;

namespace NitroxClient.Communication.Abstract
{
    public interface IMultiplayerSessionManagerState
    {
        MultiplayerSessionManagerStage CurrentStage { get; }

        void Connect(IMultiplayerSessionManager sessionManager);
        void RequestSessionReservation(IMultiplayerSessionManager sessionManager);
        void ProcessReservationResponsePacket(IMultiplayerSessionManager sessionManager);
        void JoinSession(IMultiplayerSessionManager sessionManager);
        void Disconnect(IMultiplayerSessionManager sessionManager);
    }
}
