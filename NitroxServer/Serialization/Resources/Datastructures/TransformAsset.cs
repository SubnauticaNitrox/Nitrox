using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.Serialization.Resources.Datastructures
{
    public class TransformAsset
    {
        public AssetIdentifier Identifier { get; set; }
        public AssetIdentifier ParentIdentifier { get; set; }
        public Quaternion LocalRotation { get; set; }
        public Vector3 LocalPosition { get; set; } // These were misnomers
        public Vector3 LocalScale { get; set; }
    }
}
