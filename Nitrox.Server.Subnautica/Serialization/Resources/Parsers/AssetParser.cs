using AssetsTools.NET;
using Nitrox.Server.Serialization.Resources.Datastructures;

namespace Nitrox.Server.Subnautica.Serialization.Resources.Parsers
{
    public abstract class AssetParser
    {
        public abstract void Parse(AssetIdentifier identifier, AssetsFileReader reader, ResourceAssets resourceAssets);
    }
}
