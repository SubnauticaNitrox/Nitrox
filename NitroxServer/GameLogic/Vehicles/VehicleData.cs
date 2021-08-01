using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Vehicles
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class VehicleData
    {
        [JsonProperty, ProtoMember(1)]
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
