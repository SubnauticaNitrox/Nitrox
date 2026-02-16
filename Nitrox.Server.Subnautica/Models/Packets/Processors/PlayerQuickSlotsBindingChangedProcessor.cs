using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerQuickSlotsBindingChangedProcessor : IAuthPacketProcessor<PlayerQuickSlotsBindingChanged>
{
    public Task Process(AuthProcessorContext context, PlayerQuickSlotsBindingChanged packet)
    {
        context.Sender.QuickSlotsBindingIds = packet.SlotItemIds;
        return Task.CompletedTask;
    }
}
