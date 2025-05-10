using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Model.Subnautica.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class CyclopsDamagePointRepairedProcessor : IAuthPacketProcessor<CyclopsDamagePointRepaired>
{
    public async Task Process(AuthProcessorContext context, CyclopsDamagePointRepaired packet)
    {
        await context.ReplyToOthers(packet);
    }
}
