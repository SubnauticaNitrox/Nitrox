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

            string correlationId = packet.CorrelationId;
            string playerId = packet.PlayerName;
            PlayerSlotReservation reservation = playerManager.ReservePlayerSlot(correlationId, playerId);

            Log.Info($"Reservation processed successfully: { reservation.ToString() }...");

            connection.SendPacket(reservation, null);
        }
    }
}
