using Nitrox.Model.DataStructures.Unity;

namespace NitroxServer.Resources;

public interface IPrefabAsset
{
    public NitroxTransform Transform { get; set; }
    public string ClassId { get; }
}
