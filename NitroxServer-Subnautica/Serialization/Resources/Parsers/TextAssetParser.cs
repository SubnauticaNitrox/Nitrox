using System.Collections.Generic;
using AssetsTools.NET;
using NitroxServer.Serialization.Resources.Datastructures;
using NitroxServer_Subnautica.Serialization.Resources.Parsers.Text;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers
{
    class TextAssetParser : AssetParser
    {
        private Dictionary<string, AssetParser> textParsersByAssetName = new Dictionary<string, AssetParser>()
        {
            { "EntityDistributions", new EntityDistributionsParser() }
        };

        public override void Parse(AssetIdentifier identifier, AssetsFileReader reader, ResourceAssets resourceAssets, Dictionary<int, string> relativeFileIdToPath)
        {
            string assetName = reader.ReadCountStringInt32();


            if (textParsersByAssetName.TryGetValue(assetName, out AssetParser textResourceParser))
            {
                textResourceParser.Parse(identifier, reader, resourceAssets, relativeFileIdToPath);
            }
        }
    }
}
