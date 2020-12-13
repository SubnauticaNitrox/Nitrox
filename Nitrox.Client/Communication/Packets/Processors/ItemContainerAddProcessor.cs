using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Client.GameLogic.Helper;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
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
        }
    }
}
