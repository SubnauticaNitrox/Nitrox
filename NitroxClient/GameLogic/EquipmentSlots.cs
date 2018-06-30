using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using System.Collections.Generic;
using UnityEngine;
using NitroxModel.Helper;

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

            EquippedItemData equippedItem = new EquippedItemData(ownerGuid, itemGuid, bytes,slot);
            EquipmentAddItem equipPacket = new EquipmentAddItem(equippedItem);
            packetSender.Send(equipPacket);
        }

        public void BroadcastUnequip(Pickupable pickupable, GameObject owner, string slot)
        {
            string itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            string ownerGuid = GuidHelper.GetGuid(owner);

            EquipmentRemoveItem removeEquipment = new EquipmentRemoveItem(ownerGuid, slot, itemGuid);
            packetSender.Send(removeEquipment);
        }

        public void AddItems(List<EquippedItemData> equippedItems)
        {
            ItemsContainer container = Inventory.Get().container;
            
            foreach (EquippedItemData equippedItem in equippedItems)
            {
                GameObject item = SerializationHelper.GetGameObject(equippedItem.SerializedData);
                Pickupable pickupable = item.RequireComponent<Pickupable>();
                container.UnsafeAdd(new InventoryItem(pickupable));
                Equipment equipment = Inventory.Get().equipment;
                InventoryItem inventoryItem = new InventoryItem(pickupable);
                inventoryItem.container = equipment;
                inventoryItem.item.Reparent(equipment.tr);
                Dictionary<string, InventoryItem> itemsBySlot = (Dictionary<string, InventoryItem>)equipment.ReflectionGet("equipment");
                itemsBySlot[equippedItem.Slot] = inventoryItem;
                equipment.ReflectionCall("UpdateCount", false, false, new object[] { pickupable.GetTechType(), true });
                Equipment.SendEquipmentEvent(pickupable, 0, item, equippedItem.Slot);
                equipment.ReflectionCall("NotifyEquip", false, false, new object[] { equippedItem.Slot, inventoryItem });
            }
        }
    }
}
