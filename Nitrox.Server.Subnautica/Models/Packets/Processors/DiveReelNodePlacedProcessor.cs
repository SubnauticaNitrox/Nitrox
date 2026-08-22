using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class DiveReelNodePlacedProcessor : AuthenticatedPacketProcessor<DiveReelNodePlaced>
{
    private readonly DiveReelNodeTracker nodeTracker;
    private readonly PlayerManager playerManager;

    public DiveReelNodePlacedProcessor(DiveReelNodeTracker nodeTracker, PlayerManager playerManager)
    {
        this.nodeTracker = nodeTracker;
        this.playerManager = playerManager;
    }

    public override void Process(DiveReelNodePlaced packet, Player player)
    {
        if (packet.PlayerId != player.Id)
        {
            Log.Warn($"Received {nameof(DiveReelNodePlaced)} packet where packet.{nameof(DiveReelNodePlaced.PlayerId)} was not equal to sending playerId");
            return;
        }

        nodeTracker.AddNode(packet.PlayerId, packet.Position);
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
