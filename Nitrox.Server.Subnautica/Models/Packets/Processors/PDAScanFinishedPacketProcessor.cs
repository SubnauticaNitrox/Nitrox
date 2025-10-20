using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PDAScanFinishedPacketProcessor : AuthenticatedPacketProcessor<PDAScanFinished>
{
    private readonly PlayerManager playerManager;
    private readonly PdaManager pdaManager;
    private readonly WorldEntityManager worldEntityManager;

    public PDAScanFinishedPacketProcessor(PlayerManager playerManager, PdaManager pdaManager, WorldEntityManager worldEntityManager)
    {
        this.playerManager = playerManager;
        this.pdaManager = pdaManager;
        this.worldEntityManager = worldEntityManager;
    }

    public override void Process(PDAScanFinished packet, Player player)
    {
        if (!packet.WasAlreadyResearched)
        {
            pdaManager.UpdateEntryUnlockedProgress(packet.TechType, packet.UnlockedAmount, packet.FullyResearched);
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
                pdaManager.AddScannerFragment(packet.Id);
            }
        }
    }
}
