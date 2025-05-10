using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PdaEncyclopediaEntryAddProcessor : IAuthPacketProcessor<PdaEncyclopediaEntryAdd>
{
    // TODO: USE DATABASE
    // private readonly PdaStateData pdaStateData = pdaStateData;

    public async Task Process(AuthProcessorContext context, PdaEncyclopediaEntryAdd packet)
    {
        // TODO: USE DATABASE
        // pdaStateData.AddEncyclopediaEntry(packet.Key);
        await context.ReplyToOthers(packet);
    }
}
