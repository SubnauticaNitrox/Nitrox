namespace AddressablesTools.JSON
{
#pragma warning disable IDE1006
    internal class ContentCatalogDataJson
    {
        public string m_LocatorId { get; set; }
        public ObjectInitializationDataJson m_InstanceProviderData { get; set; }
        public ObjectInitializationDataJson m_SceneProviderData { get; set; }
        public ObjectInitializationDataJson[] m_ResourceProviderData { get; set; }
        public string[] m_ProviderIds { get; set; }
        public string[] m_InternalIds { get; set; }
        public string m_KeyDataString { get; set; }
        public string m_BucketDataString { get; set; }
        public string m_EntryDataString { get; set; }
        public string m_ExtraDataString { get; set; }
        public SerializedTypeJson[] m_resourceTypes { get; set; }
        public string[] m_InternalIdPrefixes { get; set; }
    }
#pragma warning restore IDE1006
}
