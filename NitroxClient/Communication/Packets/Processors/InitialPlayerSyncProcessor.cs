using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;

namespace NitroxClient.Communication.Packets.Processors
{
    public class InitialPlayerSyncProcessor : ClientPacketProcessor<InitialPlayerSync>
    {
        private readonly BuildThrottlingQueue buildEventQueue;
        private readonly Vehicles vehicles;
        private readonly ItemContainers itemContainers;

        public InitialPlayerSyncProcessor(BuildThrottlingQueue buildEventQueue, Vehicles vehicles, ItemContainers itemContainers)
        {
            this.buildEventQueue = buildEventQueue;
            this.vehicles = vehicles;
            this.itemContainers = itemContainers;
        }

        public override void Process(InitialPlayerSync packet)
        {
            SpawnBasePieces(packet.BasePieces);
            SpawnVehicles(packet.Vehicles);
            SpawnInventoryItemsAfterBasePiecesFinish(packet.InventoryItems);
        }

        private void SpawnBasePieces(List<BasePiece> basePieces)
        {
            Log.Info("Received initial sync packet with " + basePieces.Count + " base pieces");

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

        private void SpawnVehicles(List<VehicleModel> vehicleModels)
        {
            Log.Info("Received initial sync packet with " + vehicleModels.Count + " vehicles");

            foreach (VehicleModel vehicle in vehicleModels)
            {
                vehicles.UpdateVehiclePosition(vehicle, Optional<RemotePlayer>.Empty());
            }
        }

        /*
         * Items should only be added after all base pieces spawn.  Since base pieces will spawn
         * gradually over multiple frames, we need to wait until that process has completely finished
         */
        private void SpawnInventoryItemsAfterBasePiecesFinish(List<ItemData> inventoryItems)
        {
            Log.Info("Received initial sync packet with " + inventoryItems.Count + " inventory items");

            InventoryItemAdder itemAdder = new InventoryItemAdder(itemContainers, inventoryItems);
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
            private ItemContainers itemContainers;
            private List<ItemData> inventoryItems;

            public InventoryItemAdder(ItemContainers itemContainers, List<ItemData> inventoryItems)
            {
                this.itemContainers = itemContainers;
                this.inventoryItems = inventoryItems;
            }

            public void AddItemsToInventories(object sender, EventArgs eventArgs)
            {
                Log.Info("Initial sync inventory items are clear to be added to inventories");
                ThrottledBuilder.main.QueueDrained -= AddItemsToInventories;

                foreach (ItemData itemData in inventoryItems)
                {
                    itemContainers.AddItem(itemData);
                }
            }
        }
    }
}
