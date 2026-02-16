using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerHeldItemChangedProcessor : IAuthPacketProcessor<PlayerHeldItemChanged>
{
    public async Task Process(AuthProcessorContext context, PlayerHeldItemChanged packet)
    {
        if (packet.IsFirstTime != null && !context.Sender.UsedItems.Contains(packet.IsFirstTime))
        {
            context.Sender.UsedItems.Add(packet.IsFirstTime);
        }

        await context.SendToOthersAsync(packet);
    }
}
