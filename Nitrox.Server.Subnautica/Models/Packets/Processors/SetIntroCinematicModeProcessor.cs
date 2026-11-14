using System.Linq;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SetIntroCinematicModeProcessor(PlayerManager playerManager, EscapePodManager escapePodManager, ILogger<SetIntroCinematicModeProcessor> logger) : IAuthPacketProcessor<SetIntroCinematicMode>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly EscapePodManager escapePodManager = escapePodManager;
    private readonly ILogger<SetIntroCinematicModeProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, SetIntroCinematicMode packet)
    {
        if (packet.SessionId != context.Sender.SessionId)
        {
            logger.ZLogWarning($"Received packet where {nameof(SetIntroCinematicMode.SessionId)} #{packet.SessionId} was not equal to sending {nameof(SetIntroCinematicMode.SessionId)} #{context.Sender.SessionId}");
            return;
        }

        packet.PartnerId = null; // Resetting incoming packets just to be safe we don't relay any PartnerId. Server has only authority.
        context.Sender.PlayerContext.IntroCinematicMode = packet.Mode;
        await context.SendToOthersAsync(packet);
        logger.ZLogDebug($"IntroCinematicMode set to {packet.Mode} for {context.Sender.PlayerContext.PlayerName}");

        Player[] allWaitingPlayers = playerManager.ConnectedPlayers().Where(p => p.PlayerContext.IntroCinematicMode == IntroCinematicMode.WAITING).ToArray();
        if (allWaitingPlayers.Length >= 2)
        {
            Player playerA = allWaitingPlayers[0];
            Player playerB = allWaitingPlayers[1];

            logger.ZLogInformation($"Starting IntroCinematic for {playerA.PlayerContext.PlayerName} and {playerB.PlayerContext.PlayerName}");

            playerA.PlayerContext.IntroCinematicMode = playerB.PlayerContext.IntroCinematicMode = IntroCinematicMode.START;

            await context.SendToAllAsync(new SetIntroCinematicMode(playerA.SessionId, IntroCinematicMode.START, playerB.SessionId));
            await context.SendToAllAsync(new SetIntroCinematicMode(playerB.SessionId, IntroCinematicMode.START, playerA.SessionId));

            await escapePodManager.SetupIntroSequenceAsync(playerA, playerB);
        }
    }
}
