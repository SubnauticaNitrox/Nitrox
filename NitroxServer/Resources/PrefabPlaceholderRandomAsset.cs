using System.Collections.Generic;
using Nitrox.Model.DataStructures.Unity;

namespace NitroxServer.Resources;

public record struct PrefabPlaceholderRandomAsset(List<string> ClassIds, NitroxTransform Transform = null, string ClassId = null) : IPrefabAsset
{
    public NitroxTransform Transform { get; set; } = Transform;
}
