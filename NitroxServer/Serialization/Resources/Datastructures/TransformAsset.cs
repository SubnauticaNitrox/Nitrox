using NitroxModel.DataStructures.GameLogic;
using NitroxServer.UnityStubs;

namespace NitroxServer.Serialization.Resources.Datastructures
{
    public class TransformAsset
    {
        public AssetIdentifier Identifier { get; set; }
        public AssetIdentifier ParentIdentifier { get; set; }
        public NitroxQuaternion Rotation { get; set; }
        public NitroxVector3 Position { get; set; }
        public NitroxVector3 Scale { get; set; }
    }
}
