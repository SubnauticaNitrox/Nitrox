using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Server.Subnautica.Models.Resources;

public interface IPrefabAsset
{
    public NitroxTransform Transform { get; set; }
    public string ClassId { get; }
}
