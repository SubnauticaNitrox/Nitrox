using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class EquipmentRemoveItemProcessor : ClientPacketProcessor<EquipmentRemoveItem>
    {
        public static readonly int UNEQUIP_EVENT_TYPE_ID = 1;
        
        public override void Process(EquipmentRemoveItem packet)
        {
            Optional<GameObject> opOwner = GuidHelper.GetObjectFrom(packet.OwnerGuid);

            if (opOwner.IsPresent())
            {
                GameObject owner = opOwner.Get();
                Optional<GameObject> opItem = GuidHelper.GetObjectFrom(packet.ItemGuid);

                if(opItem.IsPresent())
                {
                    GameObject item = opItem.Get();

                    Pickupable pickupable = item.GetComponent<Pickupable>();

                    if (pickupable != null)
                    {
                        Optional<Equipment> opEquipment = EquipmentHelper.GetBasedOnOwnersType(owner);

                        if (opEquipment.IsPresent())
                        {
                            Equipment equipment = opEquipment.Get();

                            Dictionary<string, InventoryItem> itemsBySlot = (Dictionary<string, InventoryItem>)equipment.ReflectionGet("equipment");
                            InventoryItem inventoryItem = itemsBySlot[packet.Slot];
                            itemsBySlot[packet.Slot] = null;

                            equipment.ReflectionCall("UpdateCount", false, false, new object[] { pickupable.GetTechType(), false });
                            Equipment.SendEquipmentEvent(pickupable, UNEQUIP_EVENT_TYPE_ID, owner, packet.Slot);
                            equipment.ReflectionCall("NotifyUnequip", false, false, new object[] { packet.Slot, inventoryItem });
                        }
                        else
                        {
                            Console.WriteLine("Could not find equipment type for " + owner.name);
                        }                        
                    }
                    else
                    {
                        Console.WriteLine("item did not have a pickupable script attached!");
                    }

                    UnityEngine.Object.Destroy(item);
                }
                else
                {
                    Console.WriteLine("Could not find item with guid " + packet.ItemGuid);
                }
            }
            else
            {
                Console.WriteLine("Could not locate equipment owner with guid: " + packet.OwnerGuid);
            }
        }
    }
}
