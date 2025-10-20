using Newtonsoft.Json;

namespace Nitrox.Server.Subnautica.Models.Resources.AddressablesTools.Json;

public class ObjectInitializationDataJson
{
    [JsonProperty("m_Id")]
    public string Id { get; set; }
    [JsonProperty("m_ObjectType")]
    public SerializedTypeJson ObjectType { get; set; }
    [JsonProperty("m_Data")]
    public string Data { get; set; }
}
