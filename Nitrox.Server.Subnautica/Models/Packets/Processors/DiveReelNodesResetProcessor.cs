using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class DiveReelNodesResetProcessor : AuthenticatedPacketProcessor<DiveReelNodesReset>
{
    private readonly DiveReelNodeTracker nodeTracker;
    private readonly PlayerManager playerManager;

    public DiveReelNodesResetProcessor(DiveReelNodeTracker nodeTracker, PlayerManager playerManager)
    {
        this.nodeTracker = nodeTracker;
        this.playerManager = playerManager;
    }

    public override void Process(DiveReelNodesReset packet, Player player)
    {
        if (packet.PlayerId != player.Id)
        {
            Log.Warn($"Received {nameof(DiveReelNodesReset)} packet where packet.{nameof(DiveReelNodesReset.PlayerId)} was not equal to sending playerId");
            return;
        }

        nodeTracker.ResetNodes(packet.PlayerId);
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
