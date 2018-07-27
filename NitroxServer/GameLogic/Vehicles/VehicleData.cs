using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Vehicles
{
    [ProtoContract]
    public class VehicleData
    {
        [ProtoMember(1)]
        public Dictionary<string, VehicleModel> SerializableBasePiecesByGuid
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

    }
}
