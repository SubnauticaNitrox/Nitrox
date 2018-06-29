using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class EquipmentSlots
    {
        private readonly IPacketSender packetSender;

        public EquipmentSlots(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void BroadcastEquip(Pickupable pickupable, GameObject owner, string slot)
        {
            string ownerGuid = GuidHelper.GetGuid(owner);
            string itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            byte[] bytes = SerializationHelper.GetBytes(pickupable.gameObject);

            ItemData itemData = new ItemData(ownerGuid, itemGuid, bytes);
            EquipmentAddItem equip = new EquipmentAddItem(itemData, slot);
            packetSender.Send(equip);
        }



        public void BroadcastUnequip(Pickupable pickupable, GameObject owner, string slot)
        {
            string itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            string ownerGuid = GuidHelper.GetGuid(owner);

            EquipmentRemoveItem removeEquipment = new EquipmentRemoveItem(ownerGuid, slot, itemGuid);
            packetSender.Send(removeEquipment);
        }

        public void AddItems(List<ItemData> items)
        {
            ItemsContainer container = Inventory.Get().container;

            foreach (ItemData itemData in items)
            {
                GameObject item = SerializationHelper.GetGameObject(itemData.SerializedData);
                Pickupable pickupable = item.RequireComponent<Pickupable>();
                container.UnsafeAdd(new InventoryItem(pickupable));
            }
        }
    }
}
