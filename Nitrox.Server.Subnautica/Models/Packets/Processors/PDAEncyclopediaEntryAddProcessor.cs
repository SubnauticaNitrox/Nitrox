using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PDAEncyclopediaEntryAddProcessor(PdaManager pdaManager) : IAuthPacketProcessor<PDAEncyclopediaEntryAdd>
{
    private readonly PdaManager pdaManager = pdaManager;

    public async Task Process(AuthProcessorContext context, PDAEncyclopediaEntryAdd packet)
    {
        pdaManager.AddEncyclopediaEntry(packet.Key);
        await context.SendToOthersAsync(packet);
    }
}
