using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System.Collections.Generic;
using UnityEngine;
using NitroxModel.Helper;
using NitroxModel.Logger;

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

            ItemEquipment itemData = new ItemEquipment(ownerGuid, itemGuid, bytes,slot);
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

        public void AddItems(List<ItemEquipment> items)
        {
            ItemsContainer container = Inventory.Get().container;


            foreach (ItemEquipment itemData in items)
            {
                GameObject item = SerializationHelper.GetGameObject(itemData.SerializedData);
                Pickupable pickupable = item.RequireComponent<Pickupable>();
                container.UnsafeAdd(new InventoryItem(pickupable));
                Equipment equipment = Inventory.Get().equipment;
                InventoryItem inventoryItem = new InventoryItem(pickupable);
                inventoryItem.container = equipment;
                inventoryItem.item.Reparent(equipment.tr);
                Dictionary<string, InventoryItem> itemsBySlot = (Dictionary<string, InventoryItem>)equipment.ReflectionGet("equipment");
                itemsBySlot[itemData.Slot] = inventoryItem;
                equipment.ReflectionCall("UpdateCount", false, false, new object[] { pickupable.GetTechType(), true });
                Equipment.SendEquipmentEvent(pickupable, 0, item, itemData.Slot);
                equipment.ReflectionCall("NotifyEquip", false, false, new object[] { itemData.Slot, inventoryItem });
            }
        }
    }
}
