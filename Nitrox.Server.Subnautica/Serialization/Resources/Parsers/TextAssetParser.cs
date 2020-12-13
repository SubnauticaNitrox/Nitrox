using System.Collections.Generic;
using AssetsTools.NET;
using Nitrox.Server.Serialization.Resources.Datastructures;
using Nitrox.Server.Subnautica.Serialization.Resources.Parsers.Text;

namespace Nitrox.Server.Subnautica.Serialization.Resources.Parsers
{
    class TextAssetParser : AssetParser
    {
        private Dictionary<string, AssetParser> textParsersByAssetName = new Dictionary<string, AssetParser>()
        {
            { "EntityDistributions", new EntityDistributionsParser() }
        };

        public override void Parse(AssetIdentifier identifier, AssetsFileReader reader, ResourceAssets resourceAssets)
        {
            string assetName = reader.ReadCountStringInt32();

            AssetParser textResourceParser;

            if (textParsersByAssetName.TryGetValue(assetName, out textResourceParser))
            {
                textResourceParser.Parse(identifier, reader, resourceAssets);
            }
        }
    }
}
