using NitroxModel.PlayerSlot;

namespace NitroxClient.Communication
{
    public interface IClientBridge : IPacketSender
    {
        ClientBridgeState CurrentState { get; }
        ReservationRejectionReason ReservationRejectionReason { get; }

        void Connect(string ipAddress, string playerName);
        void Disconnect();
    }
}