using System.Collections.Generic;
using AssetsTools.NET;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers
{
    public class GameObjectAssetParser : AssetParser
    {
        public static List<GameObjectAsset> GameObjectAssets { get; } = new List<GameObjectAsset>();

        public override void Parse(AssetIdentifier identifier, AssetsFileReader reader, ResourceAssets resourceAssets)
        {
            GameObjectAsset gameObjectAsset = new GameObjectAsset();
            gameObjectAsset.Identifier = identifier;

            uint componentCount = reader.ReadUInt32();

            for (int i = 0; i < componentCount; i++)
            {
                AssetIdentifier component = new AssetIdentifier((uint)reader.ReadInt32(), (ulong)reader.ReadInt64());
                gameObjectAsset.Components.Add(component);
            }

            GameObjectAssets.Add(gameObjectAsset);
        }
    }
}
