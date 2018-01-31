using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel.PlayerSlot;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayerSlotReservationProcessor : ClientPacketProcessor<PlayerSlotReservation>
    {
        private ClientBridge clientBridge;

        public PlayerSlotReservationProcessor(ClientBridge clientBridge)
        {
            this.clientBridge = clientBridge;
        }

        public override void Process(PlayerSlotReservation packet)
        {
            if (packet.ReservationState == PlayerSlotReservationState.Reserved)
            {
                clientBridge.ConfirmReservation(packet.CorrelationId, packet.ReservationKey);
            }
            else
            {
                clientBridge.HandleRejectedReservation(packet.CorrelationId, packet.ReservationState);
            }
        }
    }
}
