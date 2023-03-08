using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerQuickSlotsBindingChangedProcessor : AuthenticatedPacketProcessor<PlayerQuickSlotsBindingChanged>
    {
        public override void Process(PlayerQuickSlotsBindingChanged packet, Player player)
        {
            player.QuickSlotsBindingIds = packet.SlotItemIds;
        }
    }
}
