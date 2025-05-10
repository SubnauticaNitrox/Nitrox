using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Model.Subnautica.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class CyclopsFireCreatedProcessor : IAuthPacketProcessor<CyclopsFireCreated>
{
    public async Task Process(AuthProcessorContext context, CyclopsFireCreated packet)
    {
        await context.ReplyToOthers(packet);
    }
}
