using NitroxModel.PlayerSlot;

namespace NitroxClient.Communication
{
    public interface IClientBridge : IPacketSender
    {
        ClientBridgeState CurrentState { get; }
        ReservationRejectionReason ReservationRejectionReason { get; }

        void connect(string ipAddress, string playerName);
        void disconnect();
    }
}