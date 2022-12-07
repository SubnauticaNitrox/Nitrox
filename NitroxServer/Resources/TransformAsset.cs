using NitroxModel.DataStructures.Unity;

namespace NitroxServer.Resources
{
    public class TransformAsset
    {
        public NitroxVector3 LocalPosition { get; init; }
        public NitroxQuaternion LocalRotation { get; init; }
        public NitroxVector3 LocalScale { get; init; }
    }
}
