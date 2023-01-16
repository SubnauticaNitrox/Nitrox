using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class EquipmentSlots
    {        
        private readonly IPacketSender packetSender;
        private readonly Entities entities;

        public EquipmentSlots(IPacketSender packetSender, Entities entities)
        {
            this.packetSender = packetSender;
            this.entities = entities;
        }

        public void BroadcastEquip(Pickupable pickupable, GameObject owner, string slot)
        {
            NitroxId ownerId = NitroxEntity.GetId(owner);
            NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);

            Player player = owner.GetComponent<Player>();

            if (player != null)
            {
                entities.EntityMetadataChanged(player, ownerId);
            }
            else
            {
                // UWE also sends module events here as they are technically equipment of the vehicles.
                ModuleAdded moduleAdded = new(itemId, ownerId, slot);
                packetSender.Send(moduleAdded);
            }
        }

        public void BroadcastUnequip(Pickupable pickupable, GameObject owner, string slot)
        {
            NitroxId ownerId = NitroxEntity.GetId(owner);
            NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);
            Player player = owner.GetComponent<Player>();

            if (player != null)
            {
                entities.EntityMetadataChanged(player, ownerId);
            }
            else
            {
                // UWE also sends module events here as they are technically equipment of the vehicles.
                ModuleRemoved moduleRemoved = new(itemId, ownerId);
                packetSender.Send(moduleRemoved);
            }
        }

        public void AddItems(List<EquippedItemData> equippedItems)
        {
            ItemsContainer container = Inventory.Get().container;

            foreach (EquippedItemData equippedItem in equippedItems)
            {
                GameObject gameObject = SerializationHelper.GetGameObject(equippedItem.SerializedData);
                NitroxEntity.SetNewId(gameObject, equippedItem.ItemId);

                Log.Info($"EquipmentSlots/Modules: Received item add request {gameObject.name} for container {equippedItem.ContainerId}");

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

                        Dictionary<string, InventoryItem> itemsBySlot = equipment.equipment;
                        itemsBySlot[equippedItem.Slot] = inventoryItem;

                        equipment.UpdateCount(pickupable.GetTechType(), true);
                        Equipment.SendEquipmentEvent(pickupable, 0, owner, equippedItem.Slot);
                        equipment.NotifyEquip(equippedItem.Slot, inventoryItem);
                    }
                    else
                    {
                        Log.Info($"Could not find equipment type for {gameObject.name}");
                    }
                }
                else
                {
                    Log.Info($"Could not find Container for {gameObject.name}");
                }
            }
        }
    }
}
