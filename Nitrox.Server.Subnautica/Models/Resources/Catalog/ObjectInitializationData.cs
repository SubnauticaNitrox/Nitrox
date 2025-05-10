using Nitrox.Server.Subnautica.Models.Resources.Json;

namespace Nitrox.Server.Subnautica.Models.Resources.Catalog;

public class ObjectInitializationData
{
    public string Id { get; set; }
    public SerializedType ObjectType { get; set; }
    public string Data { get; set; }

    internal void Read(ObjectInitializationDataJson obj)
    {
        Id = obj.Id;
        ObjectType = new SerializedType();
        ObjectType.Read(obj.ObjectType);
        Data = obj.Data;
    }
}
