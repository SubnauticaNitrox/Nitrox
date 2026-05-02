using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class EscapePodChangedPacketProcessor : IAuthPacketProcessor<EscapePodChanged>
{
    public async Task Process(AuthProcessorContext context, EscapePodChanged packet)
    {
        context.Sender.SubRootId = packet.EscapePodId;
        await context.SendToOthersAsync(packet);
    }
}
