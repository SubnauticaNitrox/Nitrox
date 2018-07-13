using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using System.Collections.Generic;
using UnityEngine;
using NitroxModel.Helper;
using NitroxModel.DataStructures.Util;
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

            EquippedItemData equippedItem = new EquippedItemData(ownerGuid, itemGuid, bytes, slot);


            Player player = owner.GetComponent<Player>();
            if (player != null)
            {
                EquipmentAddItem equipPacket = new EquipmentAddItem(equippedItem,true);
                packetSender.Send(equipPacket);
            }
            else
            {
                EquipmentAddItem equipPacket = new EquipmentAddItem(equippedItem, false);
                packetSender.Send(equipPacket);
            }
        }

        public void BroadcastUnequip(Pickupable pickupable, GameObject owner, string slot)
        {
            string itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            string ownerGuid = GuidHelper.GetGuid(owner);

            Player player = owner.GetComponent<Player>();
            if (player != null)
            {
                EquipmentRemoveItem removeEquipment = new EquipmentRemoveItem(ownerGuid, slot, itemGuid, true);
                packetSender.Send(removeEquipment);
            }
            else
            {
                EquipmentRemoveItem removeEquipment = new EquipmentRemoveItem(ownerGuid, slot, itemGuid, false);
                packetSender.Send(removeEquipment);
            }

                
        }

        public void AddItems(List<EquippedItemData> equippedItems)
        {
            ItemsContainer container = Inventory.Get().container;
            
            foreach (EquippedItemData equippedItem in equippedItems)
            {
                GameObject gameObject = SerializationHelper.GetGameObject(equippedItem.SerializedData);
                Pickupable pickupable = gameObject.RequireComponent<Pickupable>();
                GameObject owner = GuidHelper.RequireObjectFrom(equippedItem.ContainerGuid);
                Optional<Equipment> opEquipment = EquipmentHelper.GetBasedOnOwnersType(owner);

                if (opEquipment.IsPresent())
                {
                    Equipment equipment = opEquipment.Get();
                    InventoryItem inventoryItem = new InventoryItem(pickupable);
                    inventoryItem.container = equipment;
                    inventoryItem.item.Reparent(equipment.tr);

                    Dictionary<string, InventoryItem> itemsBySlot = (Dictionary<string, InventoryItem>)equipment.ReflectionGet("equipment");
                    itemsBySlot[equippedItem.Slot] = inventoryItem;

                    equipment.ReflectionCall("UpdateCount", false, false, new object[] { pickupable.GetTechType(), true });
                    Equipment.SendEquipmentEvent(pickupable, 0, owner, equippedItem.Slot);
                    equipment.ReflectionCall("NotifyEquip", false, false, new object[] { equippedItem.Slot, inventoryItem });
                }
                else
                {
                    Log.Info("Could not find equipment type for " + gameObject.name);
                }
            }
        }
    }
}
