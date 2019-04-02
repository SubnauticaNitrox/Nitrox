using System.Collections.Generic;
using AssetsTools.NET;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers.Monobehaviours
{
    public class EntitySlotParser : MonobehaviourParser
    {
        public static HashSet<AssetIdentifier> EntitySlotGameObjects = new HashSet<AssetIdentifier>();

        public override void Parse(AssetIdentifier identifier, AssetIdentifier gameObjectIdentifier, AssetsFileReader reader, ResourceAssets resourceAssets)
        {
            EntitySlotGameObjects.Add(gameObjectIdentifier);
        }
    }
}
