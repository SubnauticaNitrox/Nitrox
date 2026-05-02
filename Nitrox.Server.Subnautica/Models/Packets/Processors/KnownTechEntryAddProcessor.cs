using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class KnownTechEntryAddProcessor(PdaManager pdaManager) : IAuthPacketProcessor<KnownTechEntryAdd>
{
    private readonly PdaManager pdaManager = pdaManager;

    public async Task Process(AuthProcessorContext context, KnownTechEntryAdd packet)
    {
        switch (packet.Category)
        {
            case KnownTechEntryAdd.EntryCategory.KNOWN:
                pdaManager.AddKnownTechType(packet.TechType, packet.PartialTechTypesToRemove);
                break;
            case KnownTechEntryAdd.EntryCategory.ANALYZED:
                pdaManager.AddAnalyzedTechType(packet.TechType);
                break;
        }

        await context.SendToOthersAsync(packet);
    }
}
