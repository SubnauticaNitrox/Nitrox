using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PDAScanFinishedPacketProcessor : AuthenticatedPacketProcessor<PDAScanFinished>
{
    private readonly PlayerManager playerManager;
    private readonly PdaStateData pdaStateData;
    private readonly WorldEntityManager worldEntityManager;

    public PDAScanFinishedPacketProcessor(PlayerManager playerManager, PdaStateData pdaStateData, WorldEntityManager worldEntityManager)
    {
        this.playerManager = playerManager;
        this.pdaStateData = pdaStateData;
        this.worldEntityManager = worldEntityManager;
    }

    public override void Process(PDAScanFinished packet, Player player)
    {
        if (!packet.WasAlreadyResearched)
        {
            pdaStateData.UpdateEntryUnlockedProgress(packet.TechType, packet.UnlockedAmount, packet.FullyResearched);
        }
        playerManager.SendPacketToOtherPlayers(packet, player);

        
        if (packet.Id != null)
        {
            if (packet.Destroy)
            {
                worldEntityManager.TryDestroyEntity(packet.Id, out _);
            }
            else
            {
                pdaStateData.AddScannerFragment(packet.Id);
            }
        }
    }
}
