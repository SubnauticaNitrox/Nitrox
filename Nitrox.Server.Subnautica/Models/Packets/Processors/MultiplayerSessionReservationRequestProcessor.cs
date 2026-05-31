using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class MultiplayerSessionReservationRequestProcessor(PlayerManager playerManager, ILogger<MultiplayerSessionReservationRequestProcessor> logger)
    : IAnonPacketProcessor<MultiplayerSessionReservationRequest>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly ILogger<MultiplayerSessionReservationRequestProcessor> logger = logger;

    public async Task Process(AnonProcessorContext context, MultiplayerSessionReservationRequest packet)
    {
        logger.ZLogInformation($"Processing reservation request from {packet.AuthenticationContext.Username}");

        string correlationId = packet.CorrelationId;
        PlayerSettings playerSettings = packet.PlayerSettings;
        AuthenticationContext authenticationContext = packet.AuthenticationContext;
        MultiplayerSessionReservation reservation = playerManager.ReservePlayerContext(
            context.Sender.SessionId,
            context.Sender.EndPoint,
            playerSettings,
            authenticationContext,
            correlationId);

        logger.ZLogInformation($"Reservation processed successfully: Username: {packet.AuthenticationContext.Username} - {reservation}");
        await context.ReplyAsync(reservation);
    }
}
