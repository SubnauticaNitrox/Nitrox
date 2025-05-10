using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Session;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class MultiplayerSessionReservationRequestProcessor(ILogger<MultiplayerSessionReservationRequestProcessor> logger) : IAnonPacketProcessor<MultiplayerSessionReservationRequest>
{
    public async Task Process(AnonProcessorContext context, MultiplayerSessionReservationRequest packet)
    {
        // TODO: USE DATABASE
        logger.LogInformation("Processing reservation request from {Username}", packet.AuthenticationContext.Username);
        // AuthenticationContext authenticationContext = packet.AuthenticationContext;
        // MultiplayerSessionReservation reservation = playerManager.ReservePlayerContext(
        //     context.Sender,
        //     packet.PlayerSettings,
        //     authenticationContext,
        //     packet.CorrelationId);
        // logger.LogInformation("Reservation processed successfully for {Username} - {Reservation}", packet.AuthenticationContext.Username, reservation);
        // await context.ReplyToSender(reservation);
    }
}
