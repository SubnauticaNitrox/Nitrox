using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.GameLogic.Spawning;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
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
        }

        public override void Process(InitialPlayerSync packet)
        {
            /*
            //First we remove all Batteries in vehicles
            using (packetSender.Suppress<StorageSlotItemRemove>())
            {                
                foreach(VehicleModel vehicle in packet.Vehicles)
                {
                    Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(vehicle.Guid);
                    if(opGameObject.IsPresent())
                    {
                        if(opGameObject.Get().GetComponent<EnergyMixin>() != null)
                        {
                            slots.RemoveItem(vehicle.Guid,true);
                        }
                        if (vehicle.InteractiveChildIdentifiers.IsPresent())
                        {
                            foreach (InteractiveChildObjectIdentifier identifier in vehicle.InteractiveChildIdentifiers.Get())
                            {
                                Optional<GameObject> opChildGameObject = GuidHelper.GetObjectFrom(identifier.Guid);
                                if (opChildGameObject.IsPresent())
                                {
                                    if (opChildGameObject.Get().GetComponent<EnergyMixin>() != null)
                                    {
                                        slots.RemoveItem(identifier.Guid,true);
                                    }
                                }
                            }
                        }
                    }
                }
            }*/
            using (packetSender.Suppress<StorageSlotItemAdd>())
            {                
                foreach (ItemData itemData in packet.StorageSlots)
                {
                    GameObject item = SerializationHelper.GetGameObject(itemData.SerializedData);
                    // Mark this entity as spawned by the server
                    item.AddComponent<NitroxEntity>();
                    item.SetNewGuid(itemData.Guid);
                    slots.AddItem(item, itemData.ContainerGuid,true);
                }
            }
        }
    }
}
