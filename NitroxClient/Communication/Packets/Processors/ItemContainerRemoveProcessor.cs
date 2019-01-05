using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ItemContainerRemoveProcessor : ClientPacketProcessor<ItemContainerRemove>
    {
        private readonly IPacketSender packetSender;
        private readonly ItemContainers itemContainer;

        public ItemContainerRemoveProcessor(IPacketSender packetSender, ItemContainers itemContainer)
        {
            this.packetSender = packetSender;
            this.itemContainer = itemContainer;
        }

        public override void Process(ItemContainerRemove packet)
        {
            itemContainer.RemoveItem(packet.OwnerGuid, packet.ItemGuid);
        }
    }
}
