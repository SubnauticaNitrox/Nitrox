using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;
using NitroxModel.Helper;
using System.Collections.Generic;

namespace NitroxClient.Communication.Packets.Processors
{
    class EquipmentAddItemProcessor : ClientPacketProcessor<EquipmentAddItem>
    {
        public static readonly int EQUIP_EVENT_TYPE_ID = 0;
        
        public override void Process(EquipmentAddItem packet)
        {
            GameObject gameObject = SerializationHelper.GetGameObject(packet.ItemBytes);

            Pickupable pickupable = gameObject.GetComponent<Pickupable>();
            Optional<GameObject> opOwner = GuidHelper.GetObjectFrom(packet.OwnerGuid);

            if (opOwner.IsPresent())
            {
                GameObject owner = opOwner.Get();

                Optional<Equipment> opEquipment = EquipmentHelper.GetBasedOnOwnersType(owner);

                if(opEquipment.IsPresent())
                {
                    Equipment equipment = opEquipment.Get();
                    InventoryItem inventoryItem = new InventoryItem(pickupable);
                    inventoryItem.container = equipment;
                    inventoryItem.item.Reparent(equipment.tr);

                    Dictionary<string, InventoryItem> itemsBySlot = (Dictionary<string, InventoryItem>)equipment.ReflectionGet("equipment");
                    itemsBySlot[packet.Slot] = inventoryItem;

                    equipment.ReflectionCall("UpdateCount", false, false, new object[] { pickupable.GetTechType(), true });
                    Equipment.SendEquipmentEvent(pickupable, EQUIP_EVENT_TYPE_ID, owner, packet.Slot);
                    equipment.ReflectionCall("NotifyEquip", false, false, new object[] { packet.Slot, inventoryItem });
                }
                else
                {
                    Console.WriteLine("Could not find equipment type for " + gameObject.name);
                }
            }
            else
            {
                Console.WriteLine("Could not locate equipment owner with guid: " + packet.OwnerGuid);
            }
        }

    }
}
