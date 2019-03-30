using System.Collections.Generic;
using AssetsTools.NET;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers.Monobehaviours
{
    public class PrefabPlaceholdersGroupParser : AssetParser
    {
        public static Dictionary<AssetIdentifier, string> PrefabByGameObjectId { get; } = new Dictionary<AssetIdentifier, string>();

        public override void Parse(AssetIdentifier gameObjectIdentifier, AssetsFileReader reader, ResourceAssets resourceAssets)
        {
            reader.Position += 16;
            reader.ReadCountStringInt32(); //Empty...
            string prefab = reader.ReadCountStringInt32();

            PrefabByGameObjectId.Add(gameObjectIdentifier, prefab);
        }
    }
}
