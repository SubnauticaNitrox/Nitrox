using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxClient.Communication.Packets.Processors
{
    class EquipmentAddItemProcessor : ClientPacketProcessor<EquipmentAddItem>
    {
        private readonly INitroxLogger log;
        public const int EQUIP_EVENT_TYPE_ID = 0;

        public EquipmentAddItemProcessor(INitroxLogger logger)
        {
            log = logger;
        }

        public override void Process(EquipmentAddItem packet)
        {
            EquippedItemData equippedItemData = packet.EquippedItemData;
            GameObject gameObject = SerializationHelper.GetGameObject(equippedItemData.SerializedData);

            Pickupable pickupable = gameObject.RequireComponent<Pickupable>();

            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(equippedItemData.ContainerGuid);

            if (opGameObject.IsPresent())
            {
                GameObject owner = opGameObject.Get();
                Optional<Equipment> opEquipment = EquipmentHelper.GetBasedOnOwnersType(owner);

                if (opEquipment.IsPresent())
                {
                    Equipment equipment = opEquipment.Get();
                    InventoryItem inventoryItem = new InventoryItem(pickupable);
                    inventoryItem.container = equipment;
                    inventoryItem.item.Reparent(equipment.tr);

                    Dictionary<string, InventoryItem> itemsBySlot = (Dictionary<string, InventoryItem>)equipment.ReflectionGet("equipment");
                    itemsBySlot[equippedItemData.Slot] = inventoryItem;

                    equipment.ReflectionCall("UpdateCount", false, false, new object[] { pickupable.GetTechType(), true });
                    Equipment.SendEquipmentEvent(pickupable, EQUIP_EVENT_TYPE_ID, owner, equippedItemData.Slot);
                    equipment.ReflectionCall("NotifyEquip", false, false, new object[] { equippedItemData.Slot, inventoryItem });
                }
                else
                {
                    log.Error($"Could not find equipment type for {gameObject.name}");
                }
            }
            else
            {
                log.Info($"Could not find Container for {gameObject.name}");
            }
        }
    }
}
