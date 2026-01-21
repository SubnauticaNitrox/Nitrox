using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class FireDousedProcessor : IAuthPacketProcessor<FireDoused>
{
    public async Task Process(AuthProcessorContext context, FireDoused packet)
    {
        await context.SendToOthersAsync(packet);
    }
}
