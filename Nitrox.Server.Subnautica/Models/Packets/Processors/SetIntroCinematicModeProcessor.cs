using System.Linq;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SetIntroCinematicModeProcessor : AuthenticatedPacketProcessor<SetIntroCinematicMode>
{
    private readonly PlayerManager playerManager;
    private readonly ILogger<SetIntroCinematicModeProcessor> logger;

    public SetIntroCinematicModeProcessor(PlayerManager playerManager, ILogger<SetIntroCinematicModeProcessor> logger)
    {
        this.playerManager = playerManager;
        this.logger = logger;
    }

    public override void Process(SetIntroCinematicMode packet, Player player)
    {
        if (packet.PlayerId != player.Id)
        {
            logger.ZLogWarning($"Received packet where {nameof(SetIntroCinematicMode.PlayerId)} was not equal to sending {nameof(SetIntroCinematicMode.PlayerId)}");
            return;
        }

        packet.PartnerId = null; // Resetting incoming packets just to be safe we don't relay any PartnerId. Server has only authority.
        player.PlayerContext.IntroCinematicMode = packet.Mode;
        playerManager.SendPacketToOtherPlayers(packet, player);
        logger.ZLogDebug($"IntroCinematicMode set to {packet.Mode} for {player.PlayerContext.PlayerName}");

        Player[] allWaitingPlayers = playerManager.ConnectedPlayers().Where(p => p.PlayerContext.IntroCinematicMode == IntroCinematicMode.WAITING).ToArray();
        if (allWaitingPlayers.Length >= 2)
        {
            logger.ZLogInformation($"Starting IntroCinematic for {allWaitingPlayers[0].PlayerContext.PlayerName} and {allWaitingPlayers[1].PlayerContext.PlayerName}");

            allWaitingPlayers[0].PlayerContext.IntroCinematicMode = allWaitingPlayers[1].PlayerContext.IntroCinematicMode = IntroCinematicMode.START;

            playerManager.SendPacketToAllPlayers(new SetIntroCinematicMode(allWaitingPlayers[0].Id, IntroCinematicMode.START, allWaitingPlayers[1].Id));
            playerManager.SendPacketToAllPlayers(new SetIntroCinematicMode(allWaitingPlayers[1].Id, IntroCinematicMode.START, allWaitingPlayers[0].Id));
        }
    }
}
