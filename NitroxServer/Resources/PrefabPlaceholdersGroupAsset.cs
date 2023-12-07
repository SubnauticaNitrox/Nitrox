using NitroxModel.DataStructures.Unity;

namespace NitroxServer.Resources;

/// <param name="PrefabAssets">
/// All attached PrefabPlaceholders (and PrefabPlaceholdersGroup). Is in sync with PrefabPlaceholdersGroup.prefabPlaceholders
/// </param>
public record struct PrefabPlaceholdersGroupAsset(string ClassId, IPrefabAsset[] PrefabAssets, NitroxTransform Transform = null) : IPrefabAsset
{
    public NitroxTransform Transform { get; set; } = Transform;
}
