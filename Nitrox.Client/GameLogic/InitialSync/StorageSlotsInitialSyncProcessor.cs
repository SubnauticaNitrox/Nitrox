using System.Collections;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.GameLogic.Helper;
using Nitrox.Client.GameLogic.InitialSync.Base;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.GameLogic.InitialSync
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
