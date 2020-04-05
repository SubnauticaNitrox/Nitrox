using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxServer.GameLogic.Items;
using UnityEngine;

namespace NitroxServer.GameLogic.Vehicles
{
    public class VehicleManager
    {
        private readonly InventoryManager inventoryManager;
        private readonly ThreadSafeDictionary<NitroxId, VehicleModel> vehiclesById;

        public VehicleManager(List<VehicleModel> vehicles, InventoryManager inventoryManager)
        {
            vehiclesById = new ThreadSafeDictionary<NitroxId, VehicleModel>(vehicles.ToDictionary(vehicle => vehicle.Id), false);
            this.inventoryManager = inventoryManager;
        }

        public IEnumerable<VehicleModel> GetVehicles()
        {
            return vehiclesById.Values;
        }

        public void UpdateVehicle(VehicleMovementData vehicleMovement)
        {
            if (vehiclesById.ContainsKey(vehicleMovement.Id))
            {
                VehicleModel vehicleModel = vehiclesById[vehicleMovement.Id];
                vehicleModel.Position = vehicleMovement.Position;
                vehicleModel.Rotation = vehicleMovement.Rotation;
                vehicleModel.Health = vehicleMovement.Health;
            }
        }

        public void UpdateVehicleChildObjects(NitroxId id, List<InteractiveChildObjectIdentifier> interactiveChildObjectIdentifier)
        {
            if (vehiclesById.ContainsKey(id))
            {
                vehiclesById[id].InteractiveChildIdentifiers.Set(interactiveChildObjectIdentifier);
            }
        }

        public void UpdateVehicleName(NitroxId id, string name)
        {
            if (vehiclesById.ContainsKey(id))
            {
                vehiclesById[id].Name = name;
            }
        }

        public void UpdateVehicleColours(int index, NitroxId id, Vector3 hsb, Color colour)
        {
            if (vehiclesById.ContainsKey(id))
            {
                Vector4 tmpVect = colour;
                vehiclesById[id].Colours[index] = tmpVect;
                vehiclesById[id].HSB[index] = hsb;
            }
        }

        public void AddVehicle(VehicleModel vehicleModel)
        {
            vehiclesById.Add(vehicleModel.Id, vehicleModel);
        }

        public void RemoveVehicle(NitroxId id)
        {
            RemoveItemsFromVehicle(id);
            vehiclesById.Remove(id);
        }

        public Optional<VehicleModel> GetVehicleModel(NitroxId id)
        {
            VehicleModel vehicleModel;
            vehiclesById.TryGetValue(id, out vehicleModel);
            return Optional.OfNullable(vehicleModel);
        }

        public Optional<T> GetVehicleModel<T>(NitroxId id) where T : VehicleModel
        {
            return Optional.OfNullable(GetVehicleModel(id).Value as T);
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
    }
}
