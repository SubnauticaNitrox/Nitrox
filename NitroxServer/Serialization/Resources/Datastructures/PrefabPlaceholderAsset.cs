namespace NitroxServer.Serialization.Resources.Datastructures
{
    public class PrefabPlaceholderAsset
    {
        public AssetIdentifier Identifier { get; set; }
        public AssetIdentifier GameObjectIdentifier { get; set; }
        public string ClassId { get; set; }
    }
}
