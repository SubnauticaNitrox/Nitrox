using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxServer.Resources;
using UWE;

namespace NitroxServer_Subnautica.Resources;

public sealed class ResourceAssets
{
    public Dictionary<string, WorldEntityInfo> WorldEntitiesByClassId { get; init; } = [];

    public string LootDistributionsJson { get; init; } = "";

    public Dictionary<string, PrefabPlaceholdersGroupAsset> PrefabPlaceholdersGroupsByGroupClassId { get; init; } = [];

    public RandomStartGenerator NitroxRandom { get; init; }

    public static void ValidateMembers(ResourceAssets resourceAssets)
    {
        Validate.NotNull(resourceAssets);
        Validate.IsTrue(resourceAssets.WorldEntitiesByClassId.Count > 1);
        Validate.IsTrue(resourceAssets.LootDistributionsJson != string.Empty);
        Validate.IsTrue(resourceAssets.PrefabPlaceholdersGroupsByGroupClassId.Count > 1);
        Validate.NotNull(resourceAssets.NitroxRandom);
    }
}
