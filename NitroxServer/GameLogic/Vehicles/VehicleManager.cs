using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxServer.GameLogic.Items;

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
            if (vehiclesById.TryGetValue(vehicleMovement.Id, out VehicleModel vehicleModel))
            {
                vehicleModel.Position = vehicleMovement.Position;
                vehicleModel.Rotation = vehicleMovement.Rotation;
            }
        }

        public void UpdateVehicleHealth(NitroxId vehicleId, float newHealth)
        {
            if (vehiclesById.TryGetValue(vehicleId, out VehicleModel vehicleModel))
            {
                if (newHealth > 0)
                {
                    vehicleModel.Health = newHealth;
                }
                else
                {
                    RemoveVehicle(vehicleId);
                }
            }
        }

        public void UpdateVehicleChildObjects(NitroxId id, List<InteractiveChildObjectIdentifier> interactiveChildObjectIdentifier)
        {
            if (vehiclesById.TryGetValue(id, out VehicleModel vehicleModel))
            {
                vehicleModel.InteractiveChildIdentifiers.Set(interactiveChildObjectIdentifier);
            }
        }

        public void UpdateVehiclePosition(NitroxId id, NitroxVector3 position)
        {
            if (vehiclesById.TryGetValue(id, out VehicleModel vehicleModel))
            {
                vehicleModel.Position = position;
            }
        }

        public void UpdateVehicleRotation(NitroxId id, NitroxQuaternion rotation)
        {
            if (vehiclesById.TryGetValue(id, out VehicleModel vehicleModel))
            {
                vehicleModel.Rotation = rotation;
            }
        }

        public void UpdateVehicleName(NitroxId id, string name)
        {
            if (vehiclesById.TryGetValue(id, out VehicleModel vehicleModel))
            {
                vehicleModel.Name = name;
            }
        }

        public void UpdateVehicleColours(int index, NitroxId id, NitroxVector3 hsb)
        {
            if (vehiclesById.TryGetValue(id, out VehicleModel vehicleModel))
            {
                vehicleModel.HSB[index] = hsb;
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
            vehiclesById.TryGetValue(id, out VehicleModel vehicleModel);
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
