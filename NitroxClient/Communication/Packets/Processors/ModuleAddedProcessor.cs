using System;
using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    internal class ModuleAddedProcessor : ClientPacketProcessor<ModuleAdded>
    {
        public const int EQUIP_EVENT_TYPE_ID = 0;

        public override void Process(ModuleAdded packet)
        {
            EquippedItemData equippedItemData = packet.EquippedItemData;
            GameObject gameObject = SerializationHelper.GetGameObject(equippedItemData.SerializedData);
            Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(equippedItemData.ContainerId);
            if (!opGameObject.HasValue)
            {
                throw new Exception("Could not find equipment container for " + gameObject.name);
            }
            Pickupable pickupable = gameObject.RequireComponent<Pickupable>();
            GameObject owner = opGameObject.Value;
            Optional<Equipment> opEquipment = EquipmentHelper.FindEquipmentComponent(owner);
            if (!opEquipment.HasValue)
            {
                throw new Exception("Could not find equipment type for " + gameObject.name);
            }

            Equipment equipment = opEquipment.Value;
            InventoryItem inventoryItem = new InventoryItem(pickupable);
            inventoryItem.container = equipment;
            inventoryItem.item.Reparent(equipment.tr);

            Dictionary<string, InventoryItem> itemsBySlot = (Dictionary<string, InventoryItem>)equipment.ReflectionGet("equipment");
            itemsBySlot[equippedItemData.Slot] = inventoryItem;

            equipment.ReflectionCall("UpdateCount", false, false, pickupable.GetTechType(), true);
            Equipment.SendEquipmentEvent(pickupable, EQUIP_EVENT_TYPE_ID, owner, equippedItemData.Slot);
            equipment.ReflectionCall("NotifyEquip", false, false, equippedItemData.Slot, inventoryItem);
        }
    }
}
