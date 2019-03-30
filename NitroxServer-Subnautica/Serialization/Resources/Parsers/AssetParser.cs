using AssetsTools.NET;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers
{
    public abstract class AssetParser
    {
        public abstract void Parse(AssetIdentifier identifier, AssetsFileReader reader, ResourceAssets resourceAssets);
    }
}
