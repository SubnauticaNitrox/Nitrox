﻿using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class EquipmentRemoveItemProcessor : ClientPacketProcessor<EquipmentRemoveItem>
    {
        private readonly INitroxLogger log;
        public const int UNEQUIP_EVENT_TYPE_ID = 1;

        public EquipmentRemoveItemProcessor(INitroxLogger logger)
        {
            log = logger;
        }

        public override void Process(EquipmentRemoveItem packet)
        {
            GameObject owner = GuidHelper.RequireObjectFrom(packet.OwnerGuid);
            GameObject item = GuidHelper.RequireObjectFrom(packet.ItemGuid);
            Pickupable pickupable = item.RequireComponent<Pickupable>();
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
                log.Error($"Could not find equipment type for {owner.name}");
            }

            Object.Destroy(item);
        }
    }
}
