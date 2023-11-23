using NitroxModel.DataStructures.Unity;

namespace NitroxServer.Resources;

public record struct PrefabPlaceholdersGroupAsset(string ClassId, IPrefabAsset[] PrefabAssets) : IPrefabAsset
{
    public NitroxTransform Transform { get; set; }
    public string ClassId { get; } = ClassId;

    /// <summary>
    /// All attached PrefabPlaceholders (and PrefabPlaceholdersGroup). Is in sync with PrefabPlaceholdersGroup.prefabPlaceholders
    /// </summary>
    public IPrefabAsset[] PrefabPlaceholders { get; } = PrefabAssets;
}
