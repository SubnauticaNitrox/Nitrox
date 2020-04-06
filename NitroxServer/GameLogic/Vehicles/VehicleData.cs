using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;
using System.Linq;

namespace NitroxServer.GameLogic.Vehicles
{
    [ProtoContract]
    public class VehicleData
    {
        [ProtoMember(1)]
        public List<VehicleModel> Vehicles = new List<VehicleModel>();
        
        public static VehicleData From(IEnumerable<VehicleModel> vehicles)
        {
            return new VehicleData
            {
                Vehicles = vehicles.ToList()
            };
        }
    }
}
