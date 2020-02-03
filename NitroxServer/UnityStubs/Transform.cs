using ProtoBufNet;

namespace NitroxServer.UnityStubs
{
    [ProtoContract]
    public class Transform
    {
        [ProtoMember(1)]
        public Vector3 localPosition { get; }

        [ProtoMember(2)]
        public Quaternion localRotation { get; }

        [ProtoMember(3)]
        public Vector3 localScale { get; }

        public Transform(Vector3 position, Vector3 scale, Quaternion rotation)
        {
            localPosition = position;
            localScale = scale;
            localRotation = rotation;
        }

        public override string ToString()
        {
            return "[Transform Position: " + localPosition + " Scale: " + localScale + " Rotation: " + localRotation + "]";
        }
    }
}
