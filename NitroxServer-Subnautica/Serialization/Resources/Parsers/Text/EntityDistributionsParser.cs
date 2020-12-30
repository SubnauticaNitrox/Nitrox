using System.Collections.Generic;
using AssetsTools.NET;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers.Text
{
    class EntityDistributionsParser : AssetParser
    {
        public override void Parse(AssetIdentifier identifier, AssetsFileReader reader, ResourceAssets resourceAssets, Dictionary<int, string> relativeFileIdToPath)
        {
            reader.Align();
            resourceAssets.LootDistributionsJson = reader.ReadCountStringInt32().Replace("\\n", "");
        }
    }
}
