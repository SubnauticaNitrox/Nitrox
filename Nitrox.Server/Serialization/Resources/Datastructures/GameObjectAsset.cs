using System.Collections.Generic;

namespace Nitrox.Server.Serialization.Resources.Datastructures
{
    public class GameObjectAsset
    {
        public AssetIdentifier Identifier;
        public string Name;
        public List<AssetIdentifier> Components { get; } = new List<AssetIdentifier>();
    }
}
