using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Model.Subnautica.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class FireCreatedProcessor : IAuthPacketProcessor<FireCreated>
{
    public async Task Process(AuthProcessorContext context, FireCreated packet)
    {
        await context.SendToOthersAsync(packet);
    }
}
