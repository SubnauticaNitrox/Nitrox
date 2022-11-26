using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxServer.Serialization.Resources.Datastructures;
using UWE;


namespace NitroxServer_Subnautica.Serialization.Resources
{
    public class ResourceAssets
    {
        public Dictionary<string, WorldEntityInfo> WorldEntitiesByClassId { get; } = new();
        public Dictionary<string, GameObjectAsset> PrefabsByClassId { get; } = new();
        public string LootDistributionsJson { get; set; } = "";
        public readonly Dictionary<string, PrefabPlaceholdersGroupAsset> PrefabPlaceholderGroupsByGroupClassId = new();

        public RandomStartGenerator NitroxRandom;

        public static void ValidateMembers(ResourceAssets resourceAssets)
        {
            Validate.NotNull(resourceAssets);
            Validate.IsTrue(resourceAssets.WorldEntitiesByClassId.Count > 0);
            Validate.IsTrue(resourceAssets.LootDistributionsJson != "");
            Validate.IsTrue(resourceAssets.PrefabPlaceholderGroupsByGroupClassId.Count > 0);
        }
    }
}
