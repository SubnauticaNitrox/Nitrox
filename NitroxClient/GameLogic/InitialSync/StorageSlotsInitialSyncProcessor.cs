using System.Collections;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    class StorageSlotsInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly StorageSlots slots;

        public StorageSlotsInitialSyncProcessor(StorageSlots slots)
        {
            this.slots = slots;

            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor));
            DependentProcessors.Add(typeof(EquippedItemInitialSyncProcessor)); // Just to be sure, for cyclops mode persistence. See "Cyclops.SetAdvancedModes"
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            int storageSlotsSynced = 0;

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
