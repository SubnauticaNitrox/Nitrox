using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using UnityEngine;

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
            GameObject item = SerializationHelper.GetGameObject(itemData.SerializedData);            
            NitroxEntity.SetNewId(item, itemData.ItemId);
            
            itemContainer.AddItem(item, itemData.ContainerId);

            // special Planting helper
            if( itemData is PlantableItemData plantableData)
            {
                item.FixPlantGrowth(plantableData);
            }
        }
    }
}
