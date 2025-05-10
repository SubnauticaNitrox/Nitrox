using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerQuickSlotsBindingChangedProcessor : IAuthPacketProcessor<PlayerQuickSlotsBindingChanged>
{
    public async Task Process(AuthProcessorContext context, PlayerQuickSlotsBindingChanged packet)
    {
        // TODO: USE DATABASE
        // player.QuickSlotsBindingIds = packet.SlotItemIds;
    }
}
