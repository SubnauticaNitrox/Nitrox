using NitroxServer.UnityStubs;

namespace NitroxServer.Serialization.Resources.Datastructures
{
    public class TransformAsset
    {
        public AssetIdentifier Identifier { get; set; }
        public AssetIdentifier ParentIdentifier { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
    }
}
