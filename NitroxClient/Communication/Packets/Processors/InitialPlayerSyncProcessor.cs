using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using UnityEngine;
using NitroxClient.Unity.Helper;
using Story;
using System.Reflection;
using NitroxModel.Helper;

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
            SetPlayerGuid(packet.PlayerGuid);
            SpawnInventoryItemsAfterBasePiecesFinish(packet.InventoryItems);
            SpawnBasePieces(packet.BasePieces);
            SpawnVehicles(packet.Vehicles);
            SpawnInventoryItemsPlayer(packet.PlayerGuid, packet.InventoryItems);
            SetEncyclopediaEntry(packet.PDAData.EncyclopediaEntries);
            SetPDAEntryComplete(packet.PDAData.UnlockedTechTypes);
            SetPDAEntryPartial(packet.PDAData.PartiallyUnlockedTechTypes);
            SetKnownTech(packet.PDAData.KnownTechTypes);
            SetPDALog(packet.PDAData.PDALogEntries);
            SpawnPlayerEquipment(packet.EquippedItems);
            SetPlayerStats(packet.PlayerStatsData);
            SetPlayerSpawn(packet.PlayerSpawnData);            
        }

        private void SetPDALog(List<PDALogEntry> logEntries)
        {
            Log.Info("Received initial sync packet with " + logEntries.Count + " pda log entries");

            using (packetSender.Suppress<PDALogEntryAdd>())
            {
                Dictionary<string, PDALog.Entry> entries = (Dictionary<string, PDALog.Entry>)(typeof(PDALog).GetField("entries", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

                foreach (PDALogEntry logEntry in logEntries)
                {
                    if (!entries.ContainsKey(logEntry.Key))
                    {
                        PDALog.EntryData entryData;
                        PDALog.GetEntryData(logEntry.Key, out entryData);
                        PDALog.Entry entry = new PDALog.Entry();
                        entry.data = entryData;
                        entry.timestamp = logEntry.Timestamp;
                        entries.Add(entryData.key, entry);

                        if (entryData.key == "Story_AuroraWarning4")
                        {
                            CrashedShipExploder.main.ReflectionCall("SwapModels", false, false, new object[] { true });
                        }
                    }
                }
            }
        }

        private void SetKnownTech(List<TechType> techTypes)
        {
            Log.Info("Received initial sync packet with " + techTypes.Count + " known tech types");

            using (packetSender.Suppress<KnownTechEntryAdd>())
            {
                foreach (TechType techType in techTypes)
                {
                    HashSet<TechType> complete = (HashSet<TechType>)(typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                    KnownTech.Add(techType, false);
                }
            }
        }
        
        private void SetPlayerStats(PlayerStatsData statsData)
        {
            if (statsData != null)
            {
                using (packetSender.Suppress<PlayerStats>())
                {
                    Player.main.oxygenMgr.AddOxygen(statsData.Oxygen);
                }
            }
        }

        private void SetPlayerSpawn(Vector3 position)
        {
            if (!(position.x == 0 && position.y == 0 && position.z == 0))
            {
                Player.main.SetPosition(position);
            }
        }

        private void SetEncyclopediaEntry(List<string> entries)
        {
            Log.Info("Received initial sync packet with " + entries.Count + " encyclopedia entries");

            using (packetSender.Suppress<PDAEncyclopediaEntryAdd>())
            {
                foreach (string entry in entries)
                {
                    PDAEncyclopedia.Add(entry, false);
                }
            }
        }
    
        private void SetPDAEntryComplete(List<TechType> pdaEntryComplete)
        {
            HashSet<TechType> complete = (HashSet<TechType>)(typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

            foreach (TechType item in pdaEntryComplete)
            {
                complete.Add(item);
            }

            Log.Info("PDAEntryComplete Save:" + pdaEntryComplete.Count + " Read Partial Client Final Count:" + complete.Count);

        }

        private void SetPDAEntryPartial(List<PDAEntry> entries)
        {
            List<PDAScanner.Entry> partial = (List<PDAScanner.Entry>)(typeof(PDAScanner).GetField("partial", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

            foreach (PDAEntry entry in entries)
            {
                partial.Add(new PDAScanner.Entry { progress = entry.Progress, techType= entry.TechType, unlocked = entry.Unlocked });
            }

            Log.Info("PDAEntryPartial Save :" + entries.Count + " Read Partial Client Final Count:" + partial.Count);
        }

        private void SetPlayerGuid(string playerguid)
        {
            GuidHelper.SetNewGuid(Player.mainObject, playerguid);
            Log.Info("Received initial sync Player Guid: " + playerguid);
        }
    
        
        private void SpawnPlayerEquipment(List<EquippedItemData> equippedItems)
        {
            Log.Info("Received initial sync packet with " + equippedItems.Count + " equipment items");

            using (packetSender.Suppress<EquipmentAddItem>())
            {
                equipment.AddItems(equippedItems);
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
                vehicles.CreateVehicle(vehicle);
            }

        }

        private void SpawnInventoryItemsPlayer(string playerGuid, List<ItemData> inventoryItems)
        {
            using (packetSender.Suppress<ItemContainerAdd>())
            {
                ItemGoalTracker itemGoalTracker = (ItemGoalTracker)typeof(ItemGoalTracker).GetField("main", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                Dictionary<TechType, List<ItemGoal>> goals = (Dictionary<TechType, List<ItemGoal>>)(typeof(ItemGoalTracker).GetField("goals", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(itemGoalTracker));

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
                        goals.Remove(pickupable.GetTechType());  // Remove Notification Goal Event On Item You Already have On Inventory
                        container.UnsafeAdd(inventoryItem);
                    }
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
