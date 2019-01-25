using ProtoBufNet;

namespace NitroxServer.UnityStubs
{
    [ProtoContract]
    public class Transform
    {
        [ProtoMember(1)]
        public Vector3 Position { get; set; }

        [ProtoMember(2)]
        public Quaternion Rotation { get; set; }

        [ProtoMember(3)]
        public Vector3 Scale { get; set; }

        [ProtoMember(4)]
        public Vector3 LocalPosition { get; set; }

        [ProtoMember(5)]
        public Quaternion LocalRotation { get; set; }

        public Transform(Vector3 position, Vector3 scale, Quaternion rotation) : this(position, scale, rotation, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
        {}

        public Transform(Vector3 position, Vector3 scale, Quaternion rotation, Vector3 localPosition, Quaternion localRotation)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
            LocalPosition = localPosition;
            LocalRotation = localRotation;
        }

        public static implicit operator Transform(UnityEngine.Transform transform)
        {
            return new Transform(transform.position, transform.localScale, transform.rotation, transform.localPosition, transform.localRotation);
        }

        public override string ToString()
        {
            return "[Transform Position: " + Position + " Scale: " + Scale + " Rotation: " + Rotation + "]";
        }
    }
}
