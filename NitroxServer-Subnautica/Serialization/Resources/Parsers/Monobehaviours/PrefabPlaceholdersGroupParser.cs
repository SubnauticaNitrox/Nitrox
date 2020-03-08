using System.Collections.Generic;
using AssetsTools.NET;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers.Monobehaviours
{
    public class PrefabPlaceholdersGroupParser : MonobehaviourParser
    {
        public static Dictionary<AssetIdentifier, List<AssetIdentifier>> PrefabPlaceholderIdsByGameObjectId { get; } = new Dictionary<AssetIdentifier, List<AssetIdentifier>>();

        public override void Parse(AssetIdentifier identifier, AssetIdentifier gameObjectIdentifier, AssetsFileReader reader, ResourceAssets resourceAssets)
        {
            List<AssetIdentifier> prefabPlaceholderIds = new List<AssetIdentifier>();

            int placeholders = reader.ReadInt32();
            
            for (int i = 0; i < placeholders; i++)
            {
                AssetIdentifier prefabPlaceholderId = new AssetIdentifier(reader.ReadInt32(), reader.ReadInt64());
                prefabPlaceholderIds.Add(prefabPlaceholderId);
            }

            PrefabPlaceholderIdsByGameObjectId.Add(gameObjectIdentifier, prefabPlaceholderIds);
        }
    }
}
