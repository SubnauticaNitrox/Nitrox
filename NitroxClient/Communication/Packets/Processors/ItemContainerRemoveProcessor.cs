using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ItemContainerRemoveProcessor : ClientPacketProcessor<ItemContainerRemove>
    {
        private readonly ItemContainers itemContainer;

        public ItemContainerRemoveProcessor(ItemContainers itemContainer)
        {
            this.itemContainer = itemContainer;
        }

        public override void Process(ItemContainerRemove packet)
        {
            itemContainer.RemoveItem(packet.OwnerId, packet.ItemId);
        }
    }
}
