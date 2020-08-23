﻿using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    class StorageSlotsInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;
        private readonly StorageSlots slots;

        public StorageSlotsInitialSyncProcessor(IPacketSender packetSender, StorageSlots slots)
        {
            this.packetSender = packetSender;
            this.slots = slots;

            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor));
            //Items with batteries can also have battery slots
            DependentProcessors.Add(typeof(InventoryItemsInitialSyncProcessor));
            DependentProcessors.Add(typeof(EquippedItemInitialSyncProcessor)); // Just to be sure, for cyclops mode persistence. See "Cyclops.SetAdvancedModes"
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            int storageSlotsSynced = 0;

            using (packetSender.Suppress<StorageSlotItemAdd>())
            {
                foreach (ItemData itemData in packet.StorageSlots)
                {
                    waitScreenItem.SetProgress(storageSlotsSynced, packet.StorageSlots.Count);

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
