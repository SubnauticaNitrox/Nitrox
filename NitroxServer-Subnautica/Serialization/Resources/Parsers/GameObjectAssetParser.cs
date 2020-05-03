using System.Collections.Generic;
using AssetsTools.NET;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers
{
    public class GameObjectAssetParser : AssetParser
    {
        public static Dictionary<AssetIdentifier, GameObjectAsset> GameObjectsByAssetId { get; } = new Dictionary<AssetIdentifier, GameObjectAsset>();

        public override void Parse(AssetIdentifier identifier, AssetsFileReader reader, ResourceAssets resourceAssets)
        {
            GameObjectAsset gameObjectAsset = new GameObjectAsset();
            gameObjectAsset.Identifier = identifier;

            uint componentCount = reader.ReadUInt32();

            for (int i = 0; i < componentCount; i++)
            {
                AssetIdentifier component = new AssetIdentifier(reader.ReadInt32(), reader.ReadInt64());
                gameObjectAsset.Components.Add(component);
            }

            reader.ReadUInt32(); // Layer (not used)

            int length = reader.ReadInt32();
            gameObjectAsset.Name = reader.ReadStringLength(length);

            GameObjectsByAssetId.Add(identifier, gameObjectAsset);
        }
    }
}
