using Newtonsoft.Json;

namespace Nitrox.Server.Subnautica.Models.Resources.AddressablesTools.Json;

public class ContentCatalogData
{
    [JsonProperty("m_LocatorId")]
    public string LocatorId { get; set; }

    [JsonProperty("m_InstanceProviderData")]
    public ObjectInitializationDataJson InstanceProviderData { get; set; }

    [JsonProperty("m_SceneProviderData")]
    public ObjectInitializationDataJson SceneProviderData { get; set; }

    [JsonProperty("m_ResourceProviderData")]
    public ObjectInitializationDataJson[] ResourceProviderData { get; set; }

    [JsonProperty("m_ProviderIds")]
    public string[] ProviderIds { get; set; }

    [JsonProperty("m_InternalIds")]
    public string[] InternalIds { get; set; }

    [JsonProperty("m_KeyDataString")]
    public string KeyDataString { get; set; }

    [JsonProperty("m_BucketDataString")]
    public string BucketDataString { get; set; }

    [JsonProperty("m_EntryDataString")]
    public string EntryDataString { get; set; }

    [JsonProperty("m_ExtraDataString")]
    public string ExtraDataString { get; set; }

    [JsonProperty("m_resourceTypes")]
    public SerializedTypeJson[] ResourceTypes { get; set; }

    [JsonProperty("m_InternalIdPrefixes")]
    public string[] InternalIdPrefixes { get; set; }
}
