using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PDAScanFinishedPacketProcessor(IPacketSender packetSender, PdaManager pdaManager, WorldEntityManager worldEntityManager) : AuthenticatedPacketProcessor<PDAScanFinished>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly PdaManager pdaManager = pdaManager;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public override void Process(PDAScanFinished packet, Player player)
    {
        if (!packet.WasAlreadyResearched)
        {
            pdaManager.UpdateEntryUnlockedProgress(packet.TechType, packet.UnlockedAmount, packet.FullyResearched);
        }
        packetSender.SendPacketToOthersAsync(packet, player.SessionId);

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
