using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.Serialization.SaveData;

[JsonObject(MemberSerialization.OptIn)]
public class VehicleData
{
    [JsonProperty]
    public List<VehicleModel> Vehicles = new();

    public static VehicleData From(IEnumerable<VehicleModel> vehicles)
    {
        return new VehicleData
        {
            Vehicles = vehicles.ToList()
        };
    }
}
