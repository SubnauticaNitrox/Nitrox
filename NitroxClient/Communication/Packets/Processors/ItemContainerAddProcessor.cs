using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ItemContainerAddProcessor : ClientPacketProcessor<ItemContainerAdd>
    {
        private readonly IPacketSender packetSender;
        private readonly ItemContainers itemContainer;

        public ItemContainerAddProcessor(IPacketSender packetSender, ItemContainers itemContainer)
        {
            this.packetSender = packetSender;
            this.itemContainer = itemContainer;
        }

        public override void Process(ItemContainerAdd packet)
        {
            ItemData itemData = packet.ItemData;
            itemContainer.AddItem(itemData);
        }
    }
}
