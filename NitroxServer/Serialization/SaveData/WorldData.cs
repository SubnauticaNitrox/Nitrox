using System.Collections.Generic;
using Newtonsoft.Json;
using NitroxModel.DataStructures;

namespace NitroxServer.Serialization.SaveData;

[JsonObject(MemberSerialization.OptIn)]
public class WorldData
{
    [JsonProperty]
    public List<NitroxInt3> ParsedBatchCells { get; set; }

    [JsonProperty]
    public VehicleData VehicleData { get; set; }

    [JsonProperty]
    public InventoryData InventoryData { get; set; }

    [JsonProperty]
    public GameData GameData { get; set; }

    [JsonProperty]
    public EscapePodData EscapePodData { get; set; }

    [JsonProperty]
    public string Seed { get; set; }

    public bool IsValid()
    {
        return ParsedBatchCells != null && // Always returns false on empty saves (sometimes also if never entered the ocean)
               VehicleData != null &&
               InventoryData != null &&
               GameData != null &&
               EscapePodData != null;
    }
}
