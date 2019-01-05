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
using NitroxClient.GameLogic.Spawning;

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
            pickupable.gameObject.transform.SetParent(null); // On Deserialized Function Try to find non-existent Parent set null to prevent that bug
            byte[] bytes = SerializationHelper.GetBytes(pickupable.gameObject);

            if (pickupable.GetTechType() == TechType.VehicleStorageModule)
            {
                List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractGuidsOfInteractiveChildren(owner);
                VehicleChildUpdate vehicleChildInteractiveData = new VehicleChildUpdate(ownerGuid,childIdentifiers);
                packetSender.Send(vehicleChildInteractiveData);
            }

            

            EquippedItemData equippedItem = new EquippedItemData(ownerGuid, itemGuid, bytes, slot);

            Player player = owner.GetComponent<Player>();
            bool isPlayerEquipment = (player != null);
            EquipmentAddItem equipPacket = new EquipmentAddItem(equippedItem, isPlayerEquipment);
            packetSender.Send(equipPacket);
        }

        public void BroadcastUnequip(Pickupable pickupable, GameObject owner, string slot)
        {
            string itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            string ownerGuid = GuidHelper.GetGuid(owner);

            if (pickupable.GetTechType() == TechType.VehicleStorageModule)
            {
                List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractGuidsOfInteractiveChildren(owner);
                VehicleChildUpdate vehicleChildInteractiveData = new VehicleChildUpdate(ownerGuid, childIdentifiers);
                packetSender.Send(vehicleChildInteractiveData);
            }

            Player player = owner.GetComponent<Player>();
            bool isPlayerEquipment = (player != null);
            EquipmentRemoveItem equipPacket = new EquipmentRemoveItem(ownerGuid, slot, itemGuid, isPlayerEquipment);
            packetSender.Send(equipPacket);
        }
       
        public void AddItems(List<EquippedItemData> equippedItems)
        {
            ItemsContainer container = Inventory.Get().container;

            foreach (EquippedItemData equippedItem in equippedItems)
            {
                GameObject gameObject = SerializationHelper.GetGameObject(equippedItem.SerializedData);

                // Mark this entity as spawned by the server
                gameObject.AddComponent<NitroxEntity>();

                Pickupable pickupable = gameObject.RequireComponent<Pickupable>();

                Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(equippedItem.ContainerGuid);

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
                else
                {
                    Log.Info("Could not find Container for " + gameObject.name);
                }
            }
        }
    }
}
