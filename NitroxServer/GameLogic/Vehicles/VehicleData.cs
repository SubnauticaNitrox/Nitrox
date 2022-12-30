using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Vehicles
{
    [DataContract]
    public class VehicleData
    {
        [DataMember(Order = 1)]
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
