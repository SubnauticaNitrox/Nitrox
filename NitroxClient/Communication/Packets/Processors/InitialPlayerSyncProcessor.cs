using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using UnityEngine;
using NitroxClient.Unity.Helper;

namespace NitroxClient.Communication.Packets.Processors
{
    public class InitialPlayerSyncProcessor : ClientPacketProcessor<InitialPlayerSync>
    {
        private readonly IPacketSender packetSender;
        private readonly BuildThrottlingQueue buildEventQueue;
        private readonly Vehicles vehicles;
        private readonly ItemContainers itemContainers;
        private readonly EquipmentSlots equipment;

        public InitialPlayerSyncProcessor(IPacketSender packetSender, BuildThrottlingQueue buildEventQueue, Vehicles vehicles, ItemContainers itemContainers, EquipmentSlots equipment)
        {
            this.packetSender = packetSender;
            this.buildEventQueue = buildEventQueue;
            this.vehicles = vehicles;
            this.itemContainers = itemContainers;
            this.equipment = equipment;
        }

        public override void Process(InitialPlayerSync packet)
        {
            SetInventoryGuid(packet.InventoryGuid);
            SpawnPlayerEquipment(packet.Equipment);
            SpawnBasePieces(packet.BasePieces);
            SpawnVehicles(packet.Vehicles);
            SpawnInventoryItemsAfterBasePiecesFinish(packet.InventoryItems);
            SpawnInventoryItemsPlayer(packet.InventoryGuid, packet.InventoryItems);
        }

        private void SetInventoryGuid(string inventoryguid)
        {
            GuidHelper.SetNewGuid(Inventory.Get().container.tr.root.gameObject, inventoryguid);
            Log.Info("Received initial sync Player InventoryGuid: " + inventoryguid + "Container Name" + Inventory.Get().container.tr.name);
        }

        private void SpawnPlayerEquipment(List<ItemEquipment> items)
        {
            Log.Info("Received initial sync packet with " + items.Count + " equipment items");

            using (packetSender.Suppress<EquipmentAddItem>())
            {
                equipment.AddItems(items);
            }
        }

        private void SpawnBasePieces(List<BasePiece> basePieces)
        {
            Log.Info("Received initial sync packet with " + basePieces.Count + " base pieces");

            using (packetSender.Suppress<ConstructionAmountChanged>())
            using (packetSender.Suppress<ConstructionCompleted>())
            using (packetSender.Suppress<PlaceBasePiece>())
            {
                foreach (BasePiece basePiece in basePieces)
                {
                    buildEventQueue.EnqueueBasePiecePlaced(basePiece);

                    if (basePiece.ConstructionCompleted)
                    {
                        buildEventQueue.EnqueueConstructionCompleted(basePiece.Guid, basePiece.NewBaseGuid);
                    }
                    else
                    {
                        buildEventQueue.EnqueueAmountChanged(basePiece.Guid, basePiece.ConstructionAmount);
                    }
                }
            }
        }

        private void SpawnVehicles(List<VehicleModel> vehicleModels)
        {
            Log.Info("Received initial sync packet with " + vehicleModels.Count + " vehicles");

            foreach (VehicleModel vehicle in vehicleModels)
            {
                vehicles.UpdateVehiclePosition(vehicle, Optional<RemotePlayer>.Empty());
            }
        }

        private void SpawnInventoryItemsPlayer(string playerGuid,List<ItemData> inventoryItems)
        {
            foreach (ItemData itemdata in inventoryItems)
            {
                if (itemdata.ContainerGuid == playerGuid)
                {
                    GameObject item = SerializationHelper.GetGameObject(itemdata.SerializedData);
                    Pickupable pickupable = item.RequireComponent<Pickupable>();
                    ItemsContainer container = Inventory.Get().container;
                    InventoryItem inventoryItem = new InventoryItem(pickupable);
                    inventoryItem.container = container;
                    inventoryItem.item.Reparent(container.tr);
                    container.UnsafeAdd(inventoryItem);
                }
            }
        }

        /*
         * Items should only be added after all base pieces spawn.  Since base pieces will spawn
         * gradually over multiple frames, we need to wait until that process has completely finished
         */
        private void SpawnInventoryItemsAfterBasePiecesFinish(List<ItemData> inventoryItems)
        {
            Log.Info("Received initial sync packet with " + inventoryItems.Count + " inventory items");

            InventoryItemAdder itemAdder = new InventoryItemAdder(packetSender, itemContainers, inventoryItems);
            ThrottledBuilder.main.QueueDrained += itemAdder.AddItemsToInventories;
        }
        
        /*
         * This class simply encapsulates a callback method that is invoked when the throttled builder
         * is completed with the initial sync of base items.  We keep this in a new class to be able to
         * hold the relevant inventory items and use them when the time comes.  This can be later extended
         * to wait on other events if need be.
         */ 
        private class InventoryItemAdder
        {
            private IPacketSender packetSender;
            private ItemContainers itemContainers;
            private List<ItemData> inventoryItems;

            public InventoryItemAdder(IPacketSender packetSender, ItemContainers itemContainers, List<ItemData> inventoryItems)
            {
                this.packetSender = packetSender;
                this.itemContainers = itemContainers;
                this.inventoryItems = inventoryItems;
            }

            public void AddItemsToInventories(object sender, EventArgs eventArgs)
            {
                Log.Info("Initial sync inventory items are clear to be added to inventories");
                ThrottledBuilder.main.QueueDrained -= AddItemsToInventories;

                using (packetSender.Suppress<ItemContainerAdd>())
                {
                    foreach (ItemData itemData in inventoryItems)
                    {
                        itemContainers.AddItem(itemData);
                    }
                }
            }
        }
    }
}
