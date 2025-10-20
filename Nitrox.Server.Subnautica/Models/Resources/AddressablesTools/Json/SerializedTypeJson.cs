using Newtonsoft.Json;

namespace Nitrox.Server.Subnautica.Models.Resources.AddressablesTools.Json;

public class SerializedTypeJson
{
    [JsonProperty("m_AssemblyName")]
    public string AssemblyName { get; set; }
    [JsonProperty("m_ClassName")]
    public string ClassName { get; set; }
}
