using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using Story;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    public class InventoryItemsInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;
        private readonly ItemContainers itemContainers;

        public InventoryItemsInitialSyncProcessor(IPacketSender packetSender, ItemContainers itemContainers)
        {
            this.packetSender = packetSender;
            this.itemContainers = itemContainers;

            DependentProcessors.Add(typeof(GlobalRootInitialSyncProcessor)); // Global root items can have inventories like the floating locker.
            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor)); // Buildings can have inventories like storage lockers.
            DependentProcessors.Add(typeof(PlayerInitialSyncProcessor)); // The player has their own inventory.
            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor)); // Vehicle can have an inventory.
            DependentProcessors.Add(typeof(EquippedItemInitialSyncProcessor)); // Vehicles can have equipped items that spawns container
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            int totalItemDataSynced = 0;

            using (packetSender.Suppress<ItemContainerAdd>())
            {
                ItemGoalTracker itemGoalTracker = (ItemGoalTracker)typeof(ItemGoalTracker).GetField("main", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                Dictionary<TechType, List<ItemGoal>> goals = (Dictionary<TechType, List<ItemGoal>>)(typeof(ItemGoalTracker).GetField("goals", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(itemGoalTracker));

                foreach (ItemData itemdata in packet.InventoryItems)
                {
                    waitScreenItem.SetProgress(totalItemDataSynced, packet.InventoryItems.Count);

                    GameObject item;

                    try
                    {
                        item = SerializationHelper.GetGameObject(itemdata.SerializedData);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error deserializing item data. Id: {itemdata.ItemId}");
                        continue;
                    }

                    Log.Debug($"Initial item data for {item.name} giving to container {itemdata.ContainerId}");

                    Pickupable pickupable = item.GetComponent<Pickupable>();

                    if (pickupable != null && itemdata.ContainerId == packet.PlayerGameObjectId)
                    {
                        goals.Remove(pickupable.GetTechType());  // Remove Notification Goal Event On Item Player Already have On Any Container

                        ItemsContainer container = Inventory.Get().container;
                        InventoryItem inventoryItem = new InventoryItem(pickupable);
                        inventoryItem.container = container;
                        inventoryItem.item.Reparent(container.tr);

                        container.UnsafeAdd(inventoryItem);
                    }
                    else
                    {
                        itemContainers.AddItem(item, itemdata.ContainerId);
                        // special Planting helper
                        if (itemdata is PlantableItemData plantableData)
                        {
                            item.FixPlantGrowth(plantableData);
                        }
                    }

                    totalItemDataSynced++;
                    yield return null;
                }
            }

            Log.Info($"Received initial sync with {totalItemDataSynced} inventory items");
        }
    }
}
