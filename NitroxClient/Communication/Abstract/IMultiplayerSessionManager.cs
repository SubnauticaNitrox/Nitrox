using NitroxModel.PlayerSlot;

namespace NitroxClient.Communication.Abstract
{
    public interface IMultiplayerSessionManager : IPacketSender
    {
        ClientBridgeState CurrentState { get; }
        PlayerSlotReservationState ReservationState { get; }

        void Connect(string ipAddress, string playerName);
        void Disconnect();
    }
}
