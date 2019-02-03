using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

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

        [ProtoMember(2)]
        public Dictionary<string, ExosuitModel> SerializableExosuitsByGuid
        {
            get
            {
                lock (exosuitsByGuid)
                {
                    return new Dictionary<string, ExosuitModel>(exosuitsByGuid);
                }
            }
            set { exosuitsByGuid = value; }
        }

        [ProtoIgnore]
        private Dictionary<string, VehicleModel> vehiclesByGuid = new Dictionary<string, VehicleModel>();
        private Dictionary<string, ExosuitModel> exosuitsByGuid = new Dictionary<string, ExosuitModel>();

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

        public void AddExosuit(ExosuitModel exosuitModel)
        {
            lock (vehiclesByGuid)
            {
                exosuitsByGuid.Add(exosuitModel.Guid, exosuitModel);
            }
        }

        public void RemoveVehicle(string guid)
        {
            lock (vehiclesByGuid)
            {
                vehiclesByGuid.Remove(guid);
            }
        }

        public List<VehicleModel> GetVehiclesForInitialSync()
        {
            lock(vehiclesByGuid)
            {
                return new List<VehicleModel>(vehiclesByGuid.Values);
            }
        }

        public List<ExosuitModel> GetExosuitsForInitialSync()
        {
            lock (exosuitsByGuid)
            {
                return new List<ExosuitModel>(exosuitsByGuid.Values);
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

        public Optional<ExosuitModel> GetExosuitModel(string guid)
        {
            lock (exosuitsByGuid)
            {
                ExosuitModel exosuitModel;

                if (exosuitsByGuid.TryGetValue(guid, out exosuitModel))
                {
                    return Optional<ExosuitModel>.OfNullable(exosuitModel);
                }
                else
                {
                    return Optional<ExosuitModel>.Empty();
                }
            }
        }

    }
}
