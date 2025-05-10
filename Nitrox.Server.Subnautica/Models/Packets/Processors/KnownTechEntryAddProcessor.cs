using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class KnownTechEntryAddProcessor : IAuthPacketProcessor<KnownTechEntryAdd>
{
    // TODO: USE DATABASE
    // private readonly PdaStateData pdaStateData = pdaStateData;

    public async Task Process(AuthProcessorContext context, KnownTechEntryAdd packet)
    {
        // TODO: USE DATABASE
        // switch (packet.Category)
        // {
        //     case KnownTechEntryAdd.EntryCategory.KNOWN:
        //         pdaStateData.AddKnownTechType(packet.TechType, packet.PartialTechTypesToRemove);
        //         break;
        //     case KnownTechEntryAdd.EntryCategory.ANALYZED:
        //         pdaStateData.AddAnalyzedTechType(packet.TechType);
        //         break;
        // }

        await context.ReplyToOthers(packet);
    }
}
