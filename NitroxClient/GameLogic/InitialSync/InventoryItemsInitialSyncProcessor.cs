using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Containers;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using Story;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    public class InventoryItemsInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly FieldInfo itemGoalTrackerMainField = typeof(ItemGoalTracker).GetField("main", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly FieldInfo itemGoalTrackerGoalsField = typeof(ItemGoalTracker).GetField("goals", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly IPacketSender packetSender;

        public InventoryItemsInitialSyncProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;

            DependentProcessors.Add(typeof(GlobalRootInitialSyncProcessor)); // Global root items can have inventories like the floating locker.
            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor)); // Buildings can have inventories like storage lockers.
            DependentProcessors.Add(typeof(PlayerInitialSyncProcessor)); // The player has their own inventory.
            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor)); // Vehicle can have an inventory.
            DependentProcessors.Add(typeof(EquippedItemInitialSyncProcessor)); // Vehicles can have equipped items that spawns container
            DependentProcessors.Add(typeof(RemotePlayerInitialSyncProcessor)); // Remote players can have inventory items
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            int totalItemDataSynced = 0;

            using (packetSender.Suppress<ItemContainerAdd>())
            {
                ItemGoalTracker itemGoalTracker = (ItemGoalTracker)itemGoalTrackerMainField.GetValue(null);
                Dictionary<TechType, List<ItemGoal>> goals = (Dictionary<TechType, List<ItemGoal>>)itemGoalTrackerGoalsField.GetValue(itemGoalTracker);

                foreach (ItemData itemData in packet.InventoryItems)
                {
                    waitScreenItem.SetProgress(totalItemDataSynced, packet.InventoryItems.Count);

                    GameObject item;

                    try
                    {
                        item = SerializationHelper.GetGameObject(itemData.SerializedData);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error deserializing item data. Id: {itemData.ItemId}");
                        continue;
                    }

                    Log.Debug($"Initial item data for {item.name} giving to container {itemData.ContainerId}");

                    Pickupable pickupable = item.GetComponent<Pickupable>();
                    Validate.NotNull(pickupable);

                    if (itemData.ContainerId == packet.PlayerGameObjectId)
                    {
                        goals.Remove(pickupable.GetTechType());  // Remove notification goal event from item player has in any container

                        ItemsContainer container = Inventory.Get().container;
                        InventoryItem inventoryItem = new InventoryItem(pickupable) { container = container };
                        inventoryItem.item.Reparent(container.tr);

                        container.UnsafeAdd(inventoryItem);
                    }
                    else if (NitroxEntity.TryGetObjectFrom(itemData.ContainerId, out GameObject containerOwner))
                    {
                        Optional<ItemsContainer> opContainer = InventoryContainerHelper.TryGetContainerByOwner(containerOwner);
                        Validate.IsPresent(opContainer);
                        opContainer.Value.UnsafeAdd(new InventoryItem(pickupable));

                        ContainerAddItemPostProcessor postProcessor = ContainerAddItemPostProcessor.From(item);
                        postProcessor.process(item, itemData);
                    }

                    totalItemDataSynced++;
                    yield return null;
                }

                foreach (NitroxTechType usedItem in packet.UsedItems)
                {
                    Player.main.AddUsedTool(usedItem.ToUnity());
                }

                string[] quickSlotsBinding = packet.QuickSlotsBinding.ToArray();
                Inventory.main.serializedQuickSlots = quickSlotsBinding;
                Inventory.main.quickSlots.RestoreBinding(quickSlotsBinding);
            }

            Log.Info($"Received initial sync with {totalItemDataSynced} inventory items");
        }
    }
}
