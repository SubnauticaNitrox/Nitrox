using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class MultiplayerSessionReservationRequestProcessor(PlayerService playerService, ILogger<MultiplayerSessionReservationRequestProcessor> logger)
    : IAnonPacketProcessor<MultiplayerSessionReservationRequest>
{
    private readonly PlayerService playerService = playerService;
    private readonly ILogger<MultiplayerSessionReservationRequestProcessor> logger = logger;

    public async Task Process(AnonProcessorContext context, MultiplayerSessionReservationRequest packet)
    {
        logger.ZLogInformation($"Processing reservation request from {packet.AuthenticationContext.Username}");

        PlayerSettings playerSettings = packet.PlayerSettings;
        AuthenticationContext authenticationContext = packet.AuthenticationContext;
        MultiplayerSessionReservation reservation = await playerService.ReservePlayerContextAsync(
            context.Sender.SessionId,
            context.Sender.EndPoint,
            playerSettings,
            authenticationContext);

        logger.ZLogInformation($"Reservation processed successfully: Username: {packet.AuthenticationContext.Username} - {reservation}");
        await context.ReplyAsync(reservation);
    }
}
