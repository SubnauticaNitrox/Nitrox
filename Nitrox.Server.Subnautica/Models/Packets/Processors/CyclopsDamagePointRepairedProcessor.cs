using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CyclopsDamagePointRepairedProcessor : IAuthPacketProcessor<CyclopsDamagePointRepaired>
{
    public async Task Process(AuthProcessorContext context, CyclopsDamagePointRepaired packet)
    {
        await context.SendToOthersAsync(packet);
    }
}
