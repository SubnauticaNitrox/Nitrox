using System.Linq;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SetIntroCinematicModeProcessor(PlayerManager playerManager, ILogger<SetIntroCinematicModeProcessor> logger) : IAuthPacketProcessor<SetIntroCinematicMode>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly ILogger<SetIntroCinematicModeProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, SetIntroCinematicMode packet)
    {
        if (packet.SessionId != context.Sender.Id)
        {
            logger.ZLogWarning($"Received packet where {nameof(SetIntroCinematicMode.SessionId)} was not equal to sending {nameof(SetIntroCinematicMode.SessionId)}");
            return;
        }

        packet.PartnerId = null; // Resetting incoming packets just to be safe we don't relay any PartnerId. Server has only authority.
        context.Sender.PlayerContext.IntroCinematicMode = packet.Mode;
        await context.SendToOthersAsync(packet);
        logger.ZLogDebug($"IntroCinematicMode set to {packet.Mode} for {context.Sender.PlayerContext.PlayerName}");

        Player[] allWaitingPlayers = playerManager.ConnectedPlayers().Where(p => p.PlayerContext.IntroCinematicMode == IntroCinematicMode.WAITING).ToArray();
        if (allWaitingPlayers.Length >= 2)
        {
            logger.ZLogInformation($"Starting IntroCinematic for {allWaitingPlayers[0].PlayerContext.PlayerName} and {allWaitingPlayers[1].PlayerContext.PlayerName}");

            allWaitingPlayers[0].PlayerContext.IntroCinematicMode = allWaitingPlayers[1].PlayerContext.IntroCinematicMode = IntroCinematicMode.START;

            await context.SendToAllAsync(new SetIntroCinematicMode(allWaitingPlayers[0].SessionId, IntroCinematicMode.START, allWaitingPlayers[1].SessionId));
            await context.SendToAllAsync(new SetIntroCinematicMode(allWaitingPlayers[1].SessionId, IntroCinematicMode.START, allWaitingPlayers[0].SessionId));
        }
    }
}
