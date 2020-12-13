using AssetsTools.NET;
using Nitrox.Server.Serialization.Resources.Datastructures;

namespace Nitrox.Server.Subnautica.Serialization.Resources.Parsers.Monobehaviours
{
    public abstract class MonobehaviourParser
    {
        public abstract void Parse(AssetIdentifier identifier, AssetIdentifier gameObjectIdentifier, AssetsFileReader reader, ResourceAssets resourceAssets);
    }
}
