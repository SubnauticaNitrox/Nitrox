using System;
using NitroxModel.Packets;
using NitroxModel.Tcp;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.Logger;

namespace NitroxServer.Communication.Packets.Processors
{
    public class ReservePlayerSlotProcessor : UnauthenticatedPacketProcessor<ReservePlayerSlot>
    {
        private PlayerManager playerManager;

        public ReservePlayerSlotProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(ReservePlayerSlot packet, Connection connection)
        {
            Log.Info("Processing reservation request...");

            var correlationId = packet.CorrelationId;
            var playerId = packet.PlayerName;
            var reservation = playerManager.ReservePlayerSlot(correlationId, playerId);

            Log.Info($"Reservation processed successfully {reservation.PlayerId} - { reservation.ReservationRejectionReason.ToString() } - { reservation.ReservationKey }...");

            connection.SendPacket(reservation, null);
        }
    }
}
