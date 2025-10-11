using Nitrox.Model.MultiplayerSession;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    public class MultiplayerSessionReservationRequestProcessor : UnauthenticatedPacketProcessor<MultiplayerSessionReservationRequest>
    {
        private readonly PlayerManager playerManager;

        public MultiplayerSessionReservationRequestProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(MultiplayerSessionReservationRequest packet, INitroxConnection connection)
        {
            Log.Info($"Processing reservation request from {packet.AuthenticationContext.Username}");

            string correlationId = packet.CorrelationId;
            PlayerSettings playerSettings = packet.PlayerSettings;
            AuthenticationContext authenticationContext = packet.AuthenticationContext;
            MultiplayerSessionReservation reservation = playerManager.ReservePlayerContext(
                connection,
                playerSettings,
                authenticationContext,
                correlationId);

            Log.Info($"Reservation processed successfully: Username: {packet.AuthenticationContext.Username} - {reservation}");

            connection.SendPacket(reservation);
        }
    }
}
