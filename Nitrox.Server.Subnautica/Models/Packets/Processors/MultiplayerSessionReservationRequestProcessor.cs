using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    sealed class MultiplayerSessionReservationRequestProcessor : UnauthenticatedPacketProcessor<MultiplayerSessionReservationRequest>
    {
        private readonly PlayerManager playerManager;
        private readonly ILogger<MultiplayerSessionReservationRequestProcessor> logger;

        public MultiplayerSessionReservationRequestProcessor(PlayerManager playerManager, ILogger<MultiplayerSessionReservationRequestProcessor> logger)
        {
            this.playerManager = playerManager;
            this.logger = logger;
        }

        public override void Process(MultiplayerSessionReservationRequest packet, INitroxConnection connection)
        {
            logger.ZLogInformation($"Processing reservation request from {packet.AuthenticationContext.Username}");

            string correlationId = packet.CorrelationId;
            PlayerSettings playerSettings = packet.PlayerSettings;
            AuthenticationContext authenticationContext = packet.AuthenticationContext;
            MultiplayerSessionReservation reservation = playerManager.ReservePlayerContext(
                connection,
                playerSettings,
                authenticationContext,
                correlationId);

            logger.ZLogInformation($"Reservation processed successfully: Username: {packet.AuthenticationContext.Username} - {reservation}");

            connection.SendPacket(reservation);
        }
    }
}
