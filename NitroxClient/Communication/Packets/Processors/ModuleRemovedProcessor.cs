using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class ModuleRemovedProcessor : ClientPacketProcessor<ModuleRemoved>
    {
        public const int UNEQUIP_EVENT_TYPE_ID = 1;

        public override void Process(ModuleRemoved packet)
        {
            GameObject owner = NitroxEntity.RequireObjectFrom(packet.OwnerId);
            Optional<Equipment> opEquipment = EquipmentHelper.FindEquipmentComponent(owner);
            if (!opEquipment.HasValue)
            {
                Log.Error("Could not find equipment type for " + owner.name);
                return;
            }
            
            GameObject item = NitroxEntity.RequireObjectFrom(packet.ItemId);
            Pickupable pickupable = item.RequireComponent<Pickupable>();
            Equipment equipment = opEquipment.Value;
            Dictionary<string, InventoryItem> itemsBySlot = (Dictionary<string, InventoryItem>)equipment.ReflectionGet("equipment");
            InventoryItem inventoryItem = itemsBySlot[packet.Slot];
            itemsBySlot[packet.Slot] = null;

            equipment.ReflectionCall("UpdateCount",false,false,pickupable.GetTechType(), false);
            Equipment.SendEquipmentEvent(pickupable, UNEQUIP_EVENT_TYPE_ID, owner, packet.Slot);
            equipment.ReflectionCall("NotifyUnequip", false, false, packet.Slot, inventoryItem);

            UnityEngine.Object.Destroy(item);
        }
    }
}
