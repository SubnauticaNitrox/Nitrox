using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors;

public class PDAScanFinishedPacketProcessor : AuthenticatedPacketProcessor<PDAScanFinished>
{
    private readonly PlayerManager playerManager;
    private readonly PDAStateData pdaStateData;
    private readonly WorldEntityManager worldEntityManager;

    public PDAScanFinishedPacketProcessor(PlayerManager playerManager, PDAStateData pdaStateData, WorldEntityManager worldEntityManager)
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
