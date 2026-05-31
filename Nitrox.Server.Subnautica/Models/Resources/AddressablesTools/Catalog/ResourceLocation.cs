using Nitrox.Server.Subnautica.Models.Resources.AddressablesTools.Json;

namespace Nitrox.Server.Subnautica.Models.Resources.AddressablesTools.Catalog;

public class ResourceLocation
{
    public string InternalId { get; set; }
    public string ProviderId { get; set; }
    public object Dependency { get; set; }
    public object Data { get; set; }
    public int HashCode { get; set; }
    public int DependencyHashCode { get; set; }
    public string PrimaryKey { get; set; }
    public SerializedTypeJson Type { get; set; }

    public ResourceLocation(string internalId, string providerId, object dependencyKey, object data, int depHashCode, object primaryKey, SerializedTypeJson resourceType)
    {
        InternalId = internalId;
        ProviderId = providerId;
        Dependency = dependencyKey;
        Data = data;
        HashCode = internalId.GetHashCode() * 31 + providerId.GetHashCode();
        DependencyHashCode = depHashCode;
        PrimaryKey = primaryKey.ToString() ?? throw new InvalidOperationException();
        Type = resourceType;
    }
}
