using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class FireDousedProcessor : IAuthPacketProcessor<FireDoused>
{
    public async Task Process(AuthProcessorContext context, FireDoused packet)
    {
        await context.ReplyToOthers(packet);
    }
}
