using NitroxModel.DataStructures.GameLogic;
using System.Collections.Generic;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using NitroxServer.GameLogic.Items;
using NitroxModel.DataStructures;
using System.Linq;

namespace NitroxServer.GameLogic.Vehicles
{
    public class VehicleManager
    {
        private InventoryManager inventoryManager;
        private Dictionary<NitroxId, VehicleModel> vehiclesById = new Dictionary<NitroxId, VehicleModel>();

        public VehicleManager(List<VehicleModel> vehicles, InventoryManager inventoryManager)
        {
            vehiclesById = vehicles.ToDictionary(vehicle => vehicle.Id);
            this.inventoryManager = inventoryManager;
        }

        public List<VehicleModel> GetVehicles()
        {
            lock(vehiclesById)
            {
                return new List<VehicleModel>(vehiclesById.Values);
            }
        }

        public void UpdateVehicle(VehicleMovementData vehicleMovement)
        {
            lock (vehiclesById)
            {
                if (vehiclesById.ContainsKey(vehicleMovement.Id))
                {
                    VehicleModel vehicleModel = vehiclesById[vehicleMovement.Id];
                    vehicleModel.Position = vehicleMovement.Position;
                    vehicleModel.Rotation = vehicleMovement.Rotation;
                    vehicleModel.Health = vehicleMovement.Health;
                }
            }
        }

        public void UpdateVehicleChildObjects(NitroxId id, List<InteractiveChildObjectIdentifier> interactiveChildObjectIdentifier)
        {
            lock (vehiclesById)
            {
                if (vehiclesById.ContainsKey(id))
                {
                    vehiclesById[id].InteractiveChildIdentifiers = interactiveChildObjectIdentifier;
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
            
            inventoryManager.StorageItemRemoved(vehicle.Id);

            foreach (InteractiveChildObjectIdentifier child in vehicle.InteractiveChildIdentifiers)
            {
                inventoryManager.StorageItemRemoved(child.Id);
            }            
        }

        public Optional<VehicleModel> GetVehicleModel(NitroxId id)
        {
            lock (vehiclesById)
            {
                VehicleModel vehicleModel;
                vehiclesById.TryGetValue(id, out vehicleModel);
                return Optional.OfNullable(vehicleModel);
            }
        }

        public Optional<T> GetVehicleModel<T>(NitroxId id) where T : VehicleModel
        {
            return Optional.OfNullable(GetVehicleModel(id).Value as T);
        }
    }
}
