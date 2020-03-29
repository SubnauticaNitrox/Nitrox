using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

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
            NitroxId ownerId = NitroxEntity.GetId(owner);
            NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);
            TechType techType = pickupable.GetTechType();

            if (techType == TechType.VehicleStorageModule)
            {
                List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractInteractiveChildren(owner);
                VehicleChildUpdate vehicleChildInteractiveData = new VehicleChildUpdate(ownerId, childIdentifiers);
                packetSender.Send(vehicleChildInteractiveData);
            }

            Transform parent = pickupable.gameObject.transform.parent;
            pickupable.gameObject.transform.SetParent(null);
            byte[] bytes = SerializationHelper.GetBytes(pickupable.gameObject);

            EquippedItemData equippedItem = new EquippedItemData(ownerId, itemId, bytes, slot, techType.Model());
            Player player = owner.GetComponent<Player>();

            if (player != null)
            {
                PlayerEquipmentAdded equipmentAdded = new PlayerEquipmentAdded(techType.Model(), equippedItem);
                packetSender.Send(equipmentAdded);
                pickupable.gameObject.transform.SetParent(parent);

                return;
            }

            ModuleAdded moduleAdded = new ModuleAdded(equippedItem);
            packetSender.Send(moduleAdded);
            pickupable.gameObject.transform.SetParent(parent);
        }

        public void BroadcastUnequip(Pickupable pickupable, GameObject owner, string slot)
        {
            NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);
            Player player = owner.GetComponent<Player>();

            if (player != null)
            {
                TechType techType = pickupable.GetTechType();
                PlayerEquipmentRemoved equipmentAdded = new PlayerEquipmentRemoved(techType.Model(), itemId);
                packetSender.Send(equipmentAdded);

                return;
            }

            NitroxId ownerId = NitroxEntity.GetId(owner);
            if (pickupable.GetTechType() == TechType.VehicleStorageModule)
            {
                List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractInteractiveChildren(owner);
                VehicleChildUpdate vehicleChildInteractiveData = new VehicleChildUpdate(ownerId, childIdentifiers);
                packetSender.Send(vehicleChildInteractiveData);
            }

            ModuleRemoved moduleRemoved = new ModuleRemoved(ownerId, slot, itemId);
            packetSender.Send(moduleRemoved);
        }

        public void AddItems(List<EquippedItemData> equippedItems)
        {
            ItemsContainer container = Inventory.Get().container;

            foreach (EquippedItemData equippedItem in equippedItems)
            {
                GameObject gameObject = SerializationHelper.GetGameObject(equippedItem.SerializedData);
                NitroxEntity.SetNewId(gameObject, equippedItem.ItemId);

                Log.Info("EquipmentSlots/Modules: Received item add request " + gameObject.name + " for container " + equippedItem.ContainerId);

                Pickupable pickupable = gameObject.RequireComponent<Pickupable>();
                Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(equippedItem.ContainerId);

                if (opGameObject.HasValue)
                {
                    GameObject owner = opGameObject.Value;

                    Optional<Equipment> opEquipment = EquipmentHelper.FindEquipmentComponent(owner);

                    if (opEquipment.HasValue)
                    {
                        Equipment equipment = opEquipment.Value;
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
