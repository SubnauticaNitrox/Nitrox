using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Vehicles
{
    [ProtoContract]
    public class VehicleData
    {
        [ProtoMember(1)]
        public List<VehicleModel> Vehicles = new List<VehicleModel>();
        
        public static VehicleData From(List<VehicleModel> vehicles)
        {
            VehicleData vehicleData = new VehicleData();
            vehicleData.Vehicles = vehicles;

            return vehicleData;
        }
    }
}
