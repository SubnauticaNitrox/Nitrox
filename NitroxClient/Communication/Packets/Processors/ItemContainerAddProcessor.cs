using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Containers;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ItemContainerAddProcessor : ClientPacketProcessor<ItemContainerAdd>
    {
        private readonly ItemContainers itemContainer;

        public ItemContainerAddProcessor(ItemContainers itemContainer)
        {
            this.itemContainer = itemContainer;
        }

        public override void Process(ItemContainerAdd packet)
        {
            ItemData itemData = packet.ItemData;
            GameObject item = SerializationHelper.GetGameObject(itemData.SerializedData);
            NitroxEntity.SetNewId(item, itemData.ItemId);

            itemContainer.AddItem(item, itemData.ContainerId);

            ContainerAddItemPostProcessor postProcessor = ContainerAddItemPostProcessor.From(item);
            postProcessor.process(item, itemData);
        }
    }
}
