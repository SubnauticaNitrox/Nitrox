using System.Collections.Generic;
using AssetsTools.NET;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers.Monobehaviours
{
    public abstract class MonobehaviourParser
    {
        public abstract void Parse(AssetIdentifier identifier, AssetIdentifier gameObjectIdentifier, AssetsFileReader reader, ResourceAssets resourceAssets, Dictionary<int, string> relativeFileIdToPath);
    }
}
