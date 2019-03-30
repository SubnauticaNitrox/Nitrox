using System.Collections.Generic;
using AssetsTools.NET;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers.Monobehaviours
{
    public class PrefabIdentifierParser : AssetParser
    {
        public static Dictionary<AssetIdentifier, string> ClassIdByGameObjectId { get; } = new Dictionary<AssetIdentifier, string>();

        public override void Parse(AssetIdentifier gameObjectIdentifier, AssetsFileReader reader, ResourceAssets resourceAssets)
        {
            string classId = reader.ReadCountStringInt32();

            ClassIdByGameObjectId.Add(gameObjectIdentifier, classId);
        }
    }
}
