using System.Collections.Generic;
using NitroxModel.Helper;
using UWE;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer_Subnautica.Serialization.Resources
{
    public class ResourceAssets
    {
        public Dictionary<string, WorldEntityInfo> WorldEntitiesByClassId { get; } = new Dictionary<string, WorldEntityInfo>();
        public Dictionary<string, GameObjectAsset> PrefabsByClassId { get; } = new Dictionary<string, GameObjectAsset>();
        public string LootDistributionsJson { get; set; } = "";
        public Dictionary<string, List<PrefabAsset>> PlaceholderPrefabsByGroupClassId = new Dictionary<string, List<PrefabAsset>>();

        public void ValidateMembers()
        {
            Validate.IsTrue(WorldEntitiesByClassId.Count > 0);
            Validate.IsTrue(LootDistributionsJson != "");
            Validate.IsTrue(PlaceholderPrefabsByGroupClassId.Count > 0);
        }
    }
}
