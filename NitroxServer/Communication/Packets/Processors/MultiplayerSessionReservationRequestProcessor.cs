using NitroxModel.Packets;
using NitroxModel.Tcp;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.Logger;

namespace NitroxServer.Communication.Packets.Processors
{
    public class MultiplayerSessionReservationRequestProcessor : UnauthenticatedPacketProcessor<MultiplayerSessionReservationRequest>
    {
        private PlayerManager playerManager;

        public MultiplayerSessionReservationRequestProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(MultiplayerSessionReservationRequest packet, Connection connection)
        {
            Log.Info("Processing reservation request...");

            string correlationId = packet.CorrelationId;
            string playerId = packet.PlayerName;
            MultiplayerSessionReservation reservation = playerManager.ReservePlayerSlot(correlationId, playerId);

            Log.Info($"Reservation processed successfully: { reservation }...");

            connection.SendPacket(reservation, null);
        }
    }
}
