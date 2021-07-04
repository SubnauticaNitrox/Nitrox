using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Containers;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
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
            DependentProcessors.Add(typeof(RemotePlayerInitialSyncProcessor)); // Remote players can have inventory items
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            int totalItemDataSynced = 0;

            HashSet<NitroxId> onlinePlayers = new HashSet<NitroxId> { packet.PlayerGameObjectId };
            onlinePlayers.AddRange(packet.RemotePlayerData.Select(playerData => playerData.PlayerContext.PlayerNitroxId));

            using (packetSender.Suppress<ItemContainerAdd>())
            {
                ItemGoalTracker itemGoalTracker = (ItemGoalTracker)typeof(ItemGoalTracker).GetField("main", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                Dictionary<TechType, List<ItemGoal>> goals = (Dictionary<TechType, List<ItemGoal>>)(typeof(ItemGoalTracker).GetField("goals", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(itemGoalTracker));

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

                    if (pickupable && itemData.ContainerId == packet.PlayerGameObjectId)
                    {
                        goals.Remove(pickupable.GetTechType());  // Remove notification goal event from item player has in any container

                        ItemsContainer container = Inventory.Get().container;
                        InventoryItem inventoryItem = new InventoryItem(pickupable) { container = container };
                        inventoryItem.item.Reparent(container.tr);

                        container.UnsafeAdd(inventoryItem);
                    }
                    else if (onlinePlayers.Any(playerId => playerId.Equals(itemData.ContainerId)))
                    {
                        itemContainers.AddItem(item, itemData.ContainerId);

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
