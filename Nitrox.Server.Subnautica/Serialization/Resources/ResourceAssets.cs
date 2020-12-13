using System.Collections.Generic;
using Nitrox.Model.Helper;
using Nitrox.Server.Serialization.Resources.Datastructures;
using UWE;

namespace Nitrox.Server.Subnautica.Serialization.Resources
{
    public class ResourceAssets
    {
        public Dictionary<string, WorldEntityInfo> WorldEntitiesByClassId { get; } = new Dictionary<string, WorldEntityInfo>();
        public Dictionary<string, GameObjectAsset> PrefabsByClassId { get; } = new Dictionary<string, GameObjectAsset>();
        public string LootDistributionsJson { get; set; } = "";
        public Dictionary<string, PrefabPlaceholdersGroupAsset> PrefabPlaceholderGroupsByGroupClassId = new Dictionary<string, PrefabPlaceholdersGroupAsset>();

        public void ValidateMembers()
        {
            Validate.IsTrue(WorldEntitiesByClassId.Count > 0);
            Validate.IsTrue(LootDistributionsJson != "");
            Validate.IsTrue(PrefabPlaceholderGroupsByGroupClassId.Count > 0);
        }
    }
}
