using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors;

public class PDAScanProgressPacketProcessor : AuthenticatedPacketProcessor<PDAScanProgress>
{
    private readonly PlayerManager playerManager;
    private readonly PDAStateData pdaStateData;
    private readonly WorldEntityManager worldEntityManager;

    public PDAScanProgressPacketProcessor(PlayerManager playerManager, PDAStateData pdaStateData, WorldEntityManager worldEntityManager)
    {
        this.playerManager = playerManager;
        this.pdaStateData = pdaStateData;
        this.worldEntityManager = worldEntityManager;
    }

    public override void Process(PDAScanProgress packet, Player player)
    {
        if (pdaStateData.UpdateScanProgress(packet.Id, packet.TechType, packet.Progress))
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
        }

        if (packet.Destroy)
        {
            pdaStateData.FinishScanProgress(packet.Id, packet.TechType, packet.Destroy, false);
            worldEntityManager.TryDestroyEntity(packet.Id, out _);
        }
    }
}
