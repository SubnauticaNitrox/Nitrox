using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel.PlayerSlot;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayerSlotReservationProcessor : ClientPacketProcessor<PlayerSlotReservation>
    {
        private MultiplayerSessionManager _multiplayerSessionManager;

        public PlayerSlotReservationProcessor(MultiplayerSessionManager multiplayerSessionManager)
        {
            this._multiplayerSessionManager = multiplayerSessionManager;
        }

        public override void Process(PlayerSlotReservation packet)
        {
            if (packet.ReservationState == PlayerSlotReservationState.Reserved)
            {
                _multiplayerSessionManager.ConfirmReservation(packet.CorrelationId, packet.ReservationKey);
            }
            else
            {
                _multiplayerSessionManager.HandleRejectedReservation(packet.CorrelationId, packet.ReservationState);
            }
        }
    }
}
