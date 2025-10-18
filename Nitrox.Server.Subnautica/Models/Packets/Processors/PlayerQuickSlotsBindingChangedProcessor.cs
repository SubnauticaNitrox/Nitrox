using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    public class PlayerQuickSlotsBindingChangedProcessor : AuthenticatedPacketProcessor<PlayerQuickSlotsBindingChanged>
    {
        public override void Process(PlayerQuickSlotsBindingChanged packet, Player player)
        {
            player.QuickSlotsBindingIds = packet.SlotItemIds;
        }
    }
}
