using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class MultiplayerSessionReservationRequestProcessor : UnauthenticatedPacketProcessor<MultiplayerSessionReservationRequest>
{
    private readonly IPacketSender packetSender;
    private readonly PlayerManager playerManager;
    private readonly ILogger<MultiplayerSessionReservationRequestProcessor> logger;

    public MultiplayerSessionReservationRequestProcessor(IPacketSender packetSender, PlayerManager playerManager, ILogger<MultiplayerSessionReservationRequestProcessor> logger)
    {
        this.packetSender = packetSender;
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

        packetSender.SendPacketAsync(reservation, connection.SessionId);
    }
}
