using System.Linq;
using Nitrox.Model.Core;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SetIntroCinematicModeProcessor(PlayerService playerService, ILogger<SetIntroCinematicModeProcessor> logger) : IAuthPacketProcessor<SetIntroCinematicMode>
{
    private readonly PlayerService playerService = playerService;
    private readonly ILogger<SetIntroCinematicModeProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, SetIntroCinematicMode packet)
    {
        if (packet.SessionId != context.Sender)
        {
            logger.ZLogWarning($"Received packet where {nameof(SetIntroCinematicMode.SessionId)} #{packet.SessionId} was not equal to sending {nameof(SetIntroCinematicMode.SessionId)} #{context.Sender}");
            return;
        }

        packet.PartnerId = null; // Resetting incoming packets just to be safe we don't relay any PartnerId. Server has only authority.
        playerService.GetProperty<IntroCinematicModeProperty>(context.Sender).Value = packet.Mode;
        await context.SendToOthersAsync(packet);
        logger.ZLogDebug($"IntroCinematicMode set to {packet.Mode} for {playerService.GetProperty<NameProperty>(context.Sender).Value}");

        SessionId[] allWaitingPlayers = playerService.GetSessionsWhereProperty<IntroCinematicModeProperty>(property => property.Value == IntroCinematicMode.WAITING).ToArray();
        if (allWaitingPlayers.Length >= 2)
        {
            logger.ZLogInformation($"Starting IntroCinematic for {playerService.GetProperty<NameProperty>(allWaitingPlayers[0]).Value} and {playerService.GetProperty<NameProperty>(allWaitingPlayers[1]).Value}");

            playerService.GetProperty<IntroCinematicModeProperty>(allWaitingPlayers[0]).Value = playerService.GetProperty<IntroCinematicModeProperty>(allWaitingPlayers[1]).Value = IntroCinematicMode.START;

            await context.SendToAllAsync(new SetIntroCinematicMode(allWaitingPlayers[0], IntroCinematicMode.START, allWaitingPlayers[1]));
            await context.SendToAllAsync(new SetIntroCinematicMode(allWaitingPlayers[1], IntroCinematicMode.START, allWaitingPlayers[0]));
        }
    }
}
