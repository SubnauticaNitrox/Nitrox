using System.Collections.Generic;
using NitroxModel.Helper;
using NitroxModel.DataStructures.GameLogic;
using UWE;
using NitroxServer.Serialization.Resources.Datastructures;


namespace NitroxServer_Subnautica.Serialization.Resources
{
    public class ResourceAssets
    {
        public Dictionary<string, WorldEntityInfo> WorldEntitiesByClassId { get; } = new Dictionary<string, WorldEntityInfo>();
        public Dictionary<string, GameObjectAsset> PrefabsByClassId { get; } = new Dictionary<string, GameObjectAsset>();
        public string LootDistributionsJson { get; set; } = "";
        public Dictionary<string, PrefabPlaceholdersGroupAsset> PrefabPlaceholderGroupsByGroupClassId = new Dictionary<string, PrefabPlaceholdersGroupAsset>();

        public RandomStartGenerator NitroxRandom;

        public static void ValidateMembers(ResourceAssets resourceAssets)
        {
            Validate.IsFalse(resourceAssets == null);
            Validate.IsTrue(resourceAssets.WorldEntitiesByClassId.Count > 0);
            Validate.IsTrue(resourceAssets.LootDistributionsJson != "");
            Validate.IsTrue(resourceAssets.PrefabPlaceholderGroupsByGroupClassId.Count > 0);
        }
    }
}
