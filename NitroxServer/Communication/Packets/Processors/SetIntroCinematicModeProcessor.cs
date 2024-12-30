using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

public class SetIntroCinematicModeProcessor : AuthenticatedPacketProcessor<SetIntroCinematicMode>
{
    private readonly PlayerManager playerManager;

    public SetIntroCinematicModeProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(SetIntroCinematicMode packet, Player player)
    {
        if (packet.PlayerId != player.Id)
        {
            Log.Warn($"Received {nameof(SetIntroCinematicMode)} packet where packet.{nameof(SetIntroCinematicMode.PlayerId)} was not equal to sending playerId");
            return;
        }

        packet.PartnerId = null; // Resetting incoming packets just to be safe we don't relay any PartnerId. Server has only authority.
        player.PlayerContext.IntroCinematicMode = packet.Mode;
        playerManager.SendPacketToOtherPlayers(packet, player);
        Log.Debug($"Set IntroCinematicMode to {packet.Mode} for {player.PlayerContext.PlayerName}");

        Player[] allWaitingPlayers = playerManager.ConnectedPlayers().Where(p => p.PlayerContext.IntroCinematicMode == IntroCinematicMode.WAITING).ToArray();
        if (allWaitingPlayers.Length >= 2)
        {
            Log.Info($"Starting IntroCinematic for {allWaitingPlayers[0].PlayerContext.PlayerName} and {allWaitingPlayers[1].PlayerContext.PlayerName}");

            allWaitingPlayers[0].PlayerContext.IntroCinematicMode = allWaitingPlayers[1].PlayerContext.IntroCinematicMode = IntroCinematicMode.START;

            playerManager.SendPacketToAllPlayers(new SetIntroCinematicMode(allWaitingPlayers[0].Id, IntroCinematicMode.START, allWaitingPlayers[1].Id));
            playerManager.SendPacketToAllPlayers(new SetIntroCinematicMode(allWaitingPlayers[1].Id, IntroCinematicMode.START, allWaitingPlayers[0].Id));
        }
    }
}
