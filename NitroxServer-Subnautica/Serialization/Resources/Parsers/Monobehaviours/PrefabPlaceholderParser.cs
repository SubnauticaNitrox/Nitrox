using System.Collections.Generic;
using AssetsTools.NET;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers.Monobehaviours
{
    public class PrefabPlaceholderParser : MonobehaviourParser
    {
        public static Dictionary<AssetIdentifier, PrefabPlaceholderAsset> PrefabPlaceholderIdToPlaceholderAsset { get; } = new Dictionary<AssetIdentifier, PrefabPlaceholderAsset>();

        public override void Parse(AssetIdentifier identifier, AssetIdentifier gameObjectIdentifier, AssetsFileReader reader, ResourceAssets resourceAssets, Dictionary<int, string> relativeFileIdToPath)
        {
            PrefabPlaceholderAsset prefabPlaceholderAsset = new PrefabPlaceholderAsset();
            prefabPlaceholderAsset.Identifier = identifier;
            prefabPlaceholderAsset.GameObjectIdentifier = gameObjectIdentifier;
            prefabPlaceholderAsset.ClassId = reader.ReadCountStringInt32();

            PrefabPlaceholderIdToPlaceholderAsset.Add(identifier, prefabPlaceholderAsset);
        }
    }
}
