using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CyclopsFireCreatedProcessor : IAuthPacketProcessor<CyclopsFireCreated>
{
    public async Task Process(AuthProcessorContext context, CyclopsFireCreated packet)
    {
        await context.SendToOthersAsync(packet);
    }
}
