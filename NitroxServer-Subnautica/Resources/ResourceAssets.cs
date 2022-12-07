using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxServer.Resources;
using UWE;

namespace NitroxServer_Subnautica.Resources
{
    public class ResourceAssets
    {
        public Dictionary<string, WorldEntityInfo> WorldEntitiesByClassId { get; init; } = new();
        public string LootDistributionsJson { get; init; } = "";
        public Dictionary<string, PrefabPlaceholdersGroupAsset> PrefabPlaceholderGroupsByGroupClassId { get; init; } = new();
        public RandomStartGenerator NitroxRandom { get; init; }

        public static void ValidateMembers(ResourceAssets resourceAssets)
        {
            Validate.NotNull(resourceAssets);
            Validate.IsTrue(resourceAssets.WorldEntitiesByClassId.Count > 0);
            Validate.IsTrue(resourceAssets.LootDistributionsJson != "");
            Validate.IsTrue(resourceAssets.PrefabPlaceholderGroupsByGroupClassId.Count > 0);
            Validate.NotNull(resourceAssets.NitroxRandom);
        }
    }
}
