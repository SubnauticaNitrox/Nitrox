using Nitrox.Server.Subnautica.Models.Resources.Json;

namespace Nitrox.Server.Subnautica.Models.Resources.Catalog;

public class SerializedType
{
    public string AssemblyName { get; set; }
    public string ClassName { get; set; }

    internal void Read(SerializedTypeJson type)
    {
        AssemblyName = type.AssemblyName;
        ClassName = type.ClassName;
    }
}