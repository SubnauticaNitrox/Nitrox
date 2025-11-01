using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using UWE;

namespace Nitrox.Server.Subnautica.Models.Resources;

public class ResourceAssets
{
    public Dictionary<string, WorldEntityInfo> WorldEntitiesByClassId { get; init; } = new();
    public string LootDistributionsJson { get; init; } = "";
    public Dictionary<string, PrefabPlaceholdersGroupAsset> PrefabPlaceholdersGroupsByGroupClassId { get; init; } = new();
    public Dictionary<string, string[]> RandomPossibilitiesByClassId { get; init; } = new();
    public RandomStartGenerator NitroxRandom { get; init; }

    public static void ValidateMembers(ResourceAssets resourceAssets)
    {
        Validate.NotNull(resourceAssets);
        Validate.IsTrue(resourceAssets.WorldEntitiesByClassId.Count > 0);
        Validate.IsTrue(resourceAssets.LootDistributionsJson != "");
        Validate.IsTrue(resourceAssets.PrefabPlaceholdersGroupsByGroupClassId.Count > 0);
        Validate.IsTrue(resourceAssets.RandomPossibilitiesByClassId.Count > 0);
        Validate.NotNull(resourceAssets.NitroxRandom);
    }
}
