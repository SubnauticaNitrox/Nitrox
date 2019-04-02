﻿using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using NitroxModel.Core;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.GameLogic.Vehicles
{
    [ProtoContract]
    public class VehicleData
    {
        [ProtoMember(1)]
        public Dictionary<string, VehicleModel> SerializableVehiclesByGuid
        {
            get
            {
                lock (vehiclesByGuid)
                {
                    return new Dictionary<string, VehicleModel>(vehiclesByGuid);
                }
            }
            set { vehiclesByGuid = value; }
        }
        [ProtoIgnore]
        private Dictionary<string, VehicleModel> vehiclesByGuid = new Dictionary<string, VehicleModel>();
        
        public void UpdateVehicle(VehicleMovementData vehicleMovement)
        {
            lock(vehiclesByGuid)
            {
                if (vehiclesByGuid.ContainsKey(vehicleMovement.Guid))
                {
                    vehiclesByGuid[vehicleMovement.Guid].Position = vehicleMovement.Position;
                    vehiclesByGuid[vehicleMovement.Guid].Rotation = vehicleMovement.Rotation;
                }
               
            }
        }

        public void UpdateVehicleChildObjects(string guid, List<InteractiveChildObjectIdentifier> interactiveChildObjectIdentifier)
        {
            lock (vehiclesByGuid)
            {
                if (vehiclesByGuid.ContainsKey(guid))
                {
                    vehiclesByGuid[guid].InteractiveChildIdentifiers = Optional<List<InteractiveChildObjectIdentifier>>.OfNullable(interactiveChildObjectIdentifier);
                }

            }
        }

        public void UpdateVehicleName(string guid, string name)
        {
            lock (vehiclesByGuid)
            {
                if (vehiclesByGuid.ContainsKey(guid))
                {
                    vehiclesByGuid[guid].Name = name;
                }

            }
        }

        public void UpdateVehicleColours(int index, string guid, Vector3 hsb, Color colour)
        {
            lock (vehiclesByGuid)
            {
                if (vehiclesByGuid.ContainsKey(guid))
                {
                    Vector4 tmpVect = colour;
                    vehiclesByGuid[guid].Colours[index] = tmpVect;
                    vehiclesByGuid[guid].HSB[index] = hsb;
                }
            }
        }

        public void AddVehicle(VehicleModel vehicleModel)
        {
            lock (vehiclesByGuid)
            {
                vehiclesByGuid.Add(vehicleModel.Guid, vehicleModel);
            }
        }

        public void RemoveVehicle(string guid)
        {
            lock (vehiclesByGuid)
            {
                RemoveItemsFromVehicle(guid);
                vehiclesByGuid.Remove(guid);
            }
        }

        private void RemoveItemsFromVehicle(string guid)
        {
            // Remove items in vehicles (for now just batteries)
            VehicleModel vehicle = vehiclesByGuid[guid];
            InventoryData data = NitroxServiceLocator.LocateService<InventoryData>();
            data.StorageItemRemoved(vehicle.Guid);
            if (vehicle.InteractiveChildIdentifiers.IsPresent())
            {
                foreach (InteractiveChildObjectIdentifier child in vehicle.InteractiveChildIdentifiers.Get())
                {
                    data.StorageItemRemoved(child.Guid);
                }
            }
        }

        public List<VehicleModel> GetVehiclesForInitialSync()
        {
            lock(vehiclesByGuid)
            {
                return new List<VehicleModel>(vehiclesByGuid.Values);
            }
        }

        public Optional<VehicleModel> GetVehicleModel(string guid)
        {
            lock(vehiclesByGuid)
            {
                VehicleModel vehicleModel;

                if(vehiclesByGuid.TryGetValue(guid, out vehicleModel))
                {
                    return Optional<VehicleModel>.OfNullable(vehicleModel);
                }
                else
                {
                    return Optional<VehicleModel>.Empty();
                }
            }
        }

        public Optional<T> GetVehicleModel<T>(string guid) where T : VehicleModel
        {
            lock (vehiclesByGuid)
            {
                VehicleModel vehicleModel;

                if (vehiclesByGuid.TryGetValue(guid, out vehicleModel) && vehicleModel is T)
                {
                    return Optional<T>.OfNullable((T)vehicleModel);
                }
                else
                {
                    return Optional<T>.Empty();
                }
            }
        }
    }
}
