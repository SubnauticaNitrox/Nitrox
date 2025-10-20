using System.Collections.Generic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Resources.Core;

namespace Nitrox.Server.Subnautica.Models.Resources;

public record struct PrefabPlaceholderRandomAsset(List<string> ClassIds, NitroxTransform Transform = null, string ClassId = null) : IPrefabAsset
{
    public NitroxTransform Transform { get; set; } = Transform;
}
