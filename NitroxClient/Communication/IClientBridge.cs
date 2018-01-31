using NitroxModel.PlayerSlot;

namespace NitroxClient.Communication
{
    public interface IClientBridge : IPacketSender
    {
        ClientBridgeState CurrentState { get; }
        PlayerSlotReservationState ReservationState { get; }

        void Connect(string ipAddress, string playerName);
        void Disconnect();
    }
}