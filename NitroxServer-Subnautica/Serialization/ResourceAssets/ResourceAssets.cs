using System.Collections.Generic;
using UWE;

namespace NitroxServer_Subnautica.Serialization.ResourceAssets
{
    public class ResourceAssets
    {
        public Dictionary<string, WorldEntityInfo> WorldEntitiesByClassId { get; } = new Dictionary<string, WorldEntityInfo>();
        public string LootDistributionsJson { get; set; } = "";
    }
}
