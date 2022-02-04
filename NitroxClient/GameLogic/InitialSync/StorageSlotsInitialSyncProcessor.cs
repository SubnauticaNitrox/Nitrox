using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    class StorageSlotsInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;
        private readonly StorageSlots slots;
        private readonly Vehicles vehicles;

        public StorageSlotsInitialSyncProcessor(IPacketSender packetSender, StorageSlots slots, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.slots = slots;
            this.vehicles = vehicles;

            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor));
            DependentProcessors.Add(typeof(InventoryItemsInitialSyncProcessor)); // Batteries can be in a battery slots from a item
            DependentProcessors.Add(typeof(EquippedItemInitialSyncProcessor)); // Just to be sure, for cyclops mode persistence. See "Cyclops.SetAdvancedModes"
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            int storageSlotsSynced = 0;

            HashSet<NitroxId> onlinePlayers = new HashSet<NitroxId> { packet.PlayerGameObjectId };
            onlinePlayers.AddRange(packet.RemotePlayerData.Select(playerData => playerData.PlayerContext.PlayerNitroxId));

            // Removes any batteries which are in inventories from offline players
            List<ItemData> currentlyIgnoredItems = packet.InventoryItems.Where(item => !onlinePlayers.Any(player => player.Equals(item.ContainerId))).ToList();
            packet.StorageSlotItems.RemoveAll(storageItem => currentlyIgnoredItems.Any(ignoredItem => ignoredItem.ItemId.Equals(storageItem.ContainerId)));

            using (packetSender.Suppress<StorageSlotItemAdd>())
            {
                foreach (ItemData itemData in packet.StorageSlotItems)
                {
                    waitScreenItem.SetProgress(storageSlotsSynced, packet.StorageSlotItems.Count);

                    GameObject item = SerializationHelper.GetGameObject(itemData.SerializedData);

                    Log.Debug($"Initial StorageSlot item data for {item.name} giving to container {itemData.ContainerId}");

                    NitroxEntity.SetNewId(item, itemData.ItemId);
                    slots.AddItem(item, itemData.ContainerId, true);

                    storageSlotsSynced++;
                    yield return null;
                }
            }
        }
    }
}
