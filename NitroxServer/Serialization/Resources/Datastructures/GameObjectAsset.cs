using System.Collections.Generic;

namespace NitroxServer.Serialization.Resources.Datastructures
{
    public class GameObjectAsset
    {
        public AssetIdentifier Identifier;
        public List<AssetIdentifier> Components { get; } = new List<AssetIdentifier>();
    }
}
