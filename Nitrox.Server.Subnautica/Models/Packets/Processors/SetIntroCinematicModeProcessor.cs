using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class SetIntroCinematicModeProcessor : IAuthPacketProcessor<SetIntroCinematicMode>
{
    public async Task Process(AuthProcessorContext context, SetIntroCinematicMode packet)
    {
        // TODO: USE DATABASE
        // if (packet.PlayerId != player.Id)
        // {
        //     Log.Warn($"Received {nameof(SetIntroCinematicMode)} packet where packet.{nameof(SetIntroCinematicMode.PlayerId)} was not equal to sending playerId");
        //     return;
        // }
        //
        // packet.PartnerId = null; // Resetting incoming packets just to be safe we don't relay any PartnerId. Server has only authority.
        // player.PlayerContext.IntroCinematicMode = packet.Mode;
        // playerService.SendPacketToOtherPlayers(packet, player);
        // Log.Debug($"Set IntroCinematicMode to {packet.Mode} for {player.PlayerContext.PlayerName}");
        //
        // NitroxServer.Player[] allWaitingPlayers = playerService.ConnectedPlayers().Where(p => p.PlayerContext.IntroCinematicMode == IntroCinematicMode.WAITING).ToArray();
        // if (allWaitingPlayers.Length >= 2)
        // {
        //     Log.Info($"Starting IntroCinematic for {allWaitingPlayers[0].PlayerContext.PlayerName} and {allWaitingPlayers[1].PlayerContext.PlayerName}");
        //
        //     allWaitingPlayers[0].PlayerContext.IntroCinematicMode = allWaitingPlayers[1].PlayerContext.IntroCinematicMode = IntroCinematicMode.START;
        //
        //     playerService.SendPacketToAllPlayers(new SetIntroCinematicMode(allWaitingPlayers[0].Id, IntroCinematicMode.START, allWaitingPlayers[1].Id));
        //     playerService.SendPacketToAllPlayers(new SetIntroCinematicMode(allWaitingPlayers[1].Id, IntroCinematicMode.START, allWaitingPlayers[0].Id));
        // }
    }
}
