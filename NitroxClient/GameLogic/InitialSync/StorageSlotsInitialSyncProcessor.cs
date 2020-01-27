using NitroxClient.Communication.Abstract;
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
        private readonly IPacketSender packetSender;
        private readonly StorageSlots slots;
        private readonly Vehicles vehicles;

        public StorageSlotsInitialSyncProcessor(IPacketSender packetSender,StorageSlots slots, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.slots = slots;
            this.vehicles = vehicles;

            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor));
            //Items with batteries can also have battery slots
            DependentProcessors.Add(typeof(InventoryItemsInitialSyncProcessor));
            DependentProcessors.Add(typeof(EquippedItemInitialSyncProcessor)); // Just to be sure, for cyclops mode persistence. See "Cyclops.SetAdvancedModes"
        }

        public override void Process(InitialPlayerSync packet)
        {            
            using (packetSender.Suppress<StorageSlotItemAdd>())
            {                
                foreach (ItemData itemData in packet.StorageSlots)
                {
                    GameObject item = SerializationHelper.GetGameObject(itemData.SerializedData);
                    NitroxEntity.SetNewId(item, itemData.ItemId);
                    slots.AddItem(item, itemData.ContainerId,true);
                }
            }
        }
    }
}
