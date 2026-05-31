using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Resources.Core;

namespace Nitrox.Server.Subnautica.Models.Resources;

/// <param name="PrefabAssets">
/// All attached PrefabPlaceholders (and PrefabPlaceholdersGroup). Is in sync with PrefabPlaceholdersGroup.prefabPlaceholders
/// </param>
[Serializable]
public record struct PrefabPlaceholdersGroupAsset(
    string ClassId,
    IPrefabAsset[] PrefabAssets,
    NitroxTransform? Transform = null
) : IPrefabAsset;
