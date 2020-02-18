using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using NitroxModel.Core;
using NitroxServer.GameLogic.Items;
using NitroxModel.DataStructures;
using System.Linq;

namespace NitroxServer.GameLogic.Vehicles
{
    [ProtoContract]
    public class VehicleData
    {
        [ProtoMember(1)]
        private Dictionary<NitroxId, VehicleModel> SerializableVehiclesById
        {
            get
            {
                lock (vehiclesById)
                {
                    serializableVehiclesById = new Dictionary<NitroxId, VehicleModel>(vehiclesById);
                    return serializableVehiclesById;
                }
            }
            set
            {
                lock (vehiclesById)
                {
                    serializableVehiclesById = vehiclesById = value;
                }
            }
        }

        private Dictionary<NitroxId, VehicleModel> serializableVehiclesById = new Dictionary<NitroxId, VehicleModel>();

        private Dictionary<NitroxId, VehicleModel> vehiclesById = new Dictionary<NitroxId, VehicleModel>();
        
        public void UpdateVehicle(VehicleMovementData vehicleMovement)
        {
            lock(vehiclesById)
            {
                if (vehiclesById.ContainsKey(vehicleMovement.Id))
                {
                    vehiclesById[vehicleMovement.Id].Position = vehicleMovement.Position;
                    vehiclesById[vehicleMovement.Id].Rotation = vehicleMovement.Rotation;
                }
               
            }
        }

        public void UpdateVehicleChildObjects(NitroxId id, List<InteractiveChildObjectIdentifier> interactiveChildObjectIdentifier)
        {
            lock (vehiclesById)
            {
                if (vehiclesById.ContainsKey(id))
                {
                    vehiclesById[id].InteractiveChildIdentifiers = Optional<List<InteractiveChildObjectIdentifier>>.OfNullable(interactiveChildObjectIdentifier);
                }

            }
        }

        public void UpdateVehicleName(NitroxId id, string name)
        {
            lock (vehiclesById)
            {
                if (vehiclesById.ContainsKey(id))
                {
                    vehiclesById[id].Name = name;
                }

            }
        }

        public void UpdateVehicleColours(int index, NitroxId id, Vector3 hsb, Color colour)
        {
            lock (vehiclesById)
            {
                if (vehiclesById.ContainsKey(id))
                {
                    Vector4 tmpVect = colour;
                    vehiclesById[id].Colours[index] = tmpVect;
                    vehiclesById[id].HSB[index] = hsb;
                }
            }
        }

        public void AddVehicle(VehicleModel vehicleModel)
        {
            lock (vehiclesById)
            {
                vehiclesById.Add(vehicleModel.Id, vehicleModel);
            }
        }

        public void RemoveVehicle(NitroxId id)
        {
            lock (vehiclesById)
            {
                RemoveItemsFromVehicle(id);
                vehiclesById.Remove(id);
            }
        }

        private void RemoveItemsFromVehicle(NitroxId id)
        {
            // Remove items in vehicles (for now just batteries)
            VehicleModel vehicle = vehiclesById[id];

            InventoryManager inventoryManager = NitroxServiceLocator.LocateService<InventoryManager>();
            inventoryManager.StorageItemRemoved(vehicle.Id);

            if (vehicle.InteractiveChildIdentifiers.IsPresent())
            {
                foreach (InteractiveChildObjectIdentifier child in vehicle.InteractiveChildIdentifiers.Get())
                {
                    inventoryManager.StorageItemRemoved(child.Id);
                }
            }
        }

        public List<VehicleModel> GetVehiclesForInitialSync()
        {
            lock(vehiclesById)
            {
                return new List<VehicleModel>(vehiclesById.Values);
            }
        }

        public Optional<VehicleModel> GetVehicleModel(NitroxId id)
        {
            lock(vehiclesById)
            {
                VehicleModel vehicleModel;

                if(vehiclesById.TryGetValue(id, out vehicleModel))
                {
                    return Optional<VehicleModel>.OfNullable(vehicleModel);
                }
                else
                {
                    return Optional<VehicleModel>.Empty();
                }
            }
        }

        public Optional<T> GetVehicleModel<T>(NitroxId id) where T : VehicleModel
        {
            lock (vehiclesById)
            {
                VehicleModel vehicleModel;

                if (vehiclesById.TryGetValue(id, out vehicleModel) && vehicleModel is T)
                {
                    return Optional<T>.OfNullable((T)vehicleModel);
                }
                else
                {
                    return Optional<T>.Empty();
                }
            }
        }

        [ProtoAfterDeserialization]
        private void AfterDeserialization()
        {
            lock (vehiclesById)
            {
                vehiclesById = serializableVehiclesById;
            }
        }
    }
}
