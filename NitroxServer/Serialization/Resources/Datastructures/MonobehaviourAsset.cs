namespace NitroxServer.Serialization.Resources.Datastructures
{
    public class MonobehaviourAsset
    {
        public string Name { get; set; }
        public string MonoscriptName { get; set; }
        public AssetIdentifier GameObjectIdentifier { get; set; }
        public bool Enabled { get; set; }
        public AssetIdentifier MonoscriptIdentifier { get; set; }
    }
}
