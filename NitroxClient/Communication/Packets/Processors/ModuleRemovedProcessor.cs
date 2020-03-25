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
            GameObject item = NitroxEntity.RequireObjectFrom(packet.ItemId);
            Pickupable pickupable = item.RequireComponent<Pickupable>();
            Optional<Equipment> opEquipment = EquipmentHelper.FindEquipmentComponent(owner);

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
                Log.Error("Could not find equipment type for " + owner.name);
            }

            UnityEngine.Object.Destroy(item);
        }
    }
}
