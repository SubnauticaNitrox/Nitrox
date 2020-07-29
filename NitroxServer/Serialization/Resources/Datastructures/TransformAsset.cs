using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.Serialization.Resources.Datastructures
{
    [ProtoContract]
    public class TransformAsset
    {
        public AssetIdentifier Identifier { get; set; }
        public AssetIdentifier GameObjectIdentifier { get; set; }
        public AssetIdentifier ParentIdentifier { get; set; }

        [ProtoMember(1)]
        public NitroxVector3 LocalPosition { get; set; }

        [ProtoMember(2)]
        public NitroxQuaternion LocalRotation { get; set; }

        [ProtoMember(3)]
        public NitroxVector3 LocalScale { get; set; }
    }
}
