using System.Collections.Generic;
using NitroxModel.Helper;
using UWE;
using static NitroxServer_Subnautica.Serialization.Resources.Processing.PrefabPlaceholderExtractor;

namespace NitroxServer_Subnautica.Serialization.Resources
{
    public class ResourceAssets
    {
        public Dictionary<string, WorldEntityInfo> WorldEntitiesByClassId { get; } = new Dictionary<string, WorldEntityInfo>();
        public string LootDistributionsJson { get; set; } = "";
        public Dictionary<string, List<PrefabAsset>> PrefabsByClassId = new Dictionary<string, List<PrefabAsset>>();
        
        public void ValidateMembers()
        {
            Validate.IsTrue(WorldEntitiesByClassId.Count > 0);
            Validate.IsTrue(LootDistributionsJson != "");
            Validate.IsTrue(PrefabsByClassId.Count > 0);
        }
    }
}
