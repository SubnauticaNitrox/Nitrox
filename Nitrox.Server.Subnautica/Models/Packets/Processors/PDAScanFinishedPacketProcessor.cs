using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PDAScanFinishedPacketProcessor(PdaManager pdaManager, WorldEntityManager worldEntityManager) : IAuthPacketProcessor<PDAScanFinished>
{
    private readonly PdaManager pdaManager = pdaManager;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, PDAScanFinished packet)
    {
        if (!packet.WasAlreadyResearched)
        {
            pdaManager.UpdateEntryUnlockedProgress(packet.TechType, packet.UnlockedAmount, packet.FullyResearched);
        }
        await context.SendToOthersAsync(packet);

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
