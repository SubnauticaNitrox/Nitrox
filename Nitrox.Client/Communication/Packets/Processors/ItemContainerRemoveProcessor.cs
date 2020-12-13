using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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
