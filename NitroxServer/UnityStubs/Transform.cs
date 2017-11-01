using ProtoBufNet;

namespace NitroxServer.UnityStubs
{
    [ProtoContract]
    public class Transform
    {
        [ProtoMember(1)]
        public Vector3 Position { get; private set; }

        [ProtoMember(2)]
        public Vector3 Scale { get; private set; }

        [ProtoMember(3)]
        public Quaternion Rotation { get; private set; }

        public Transform(Vector3 position, Vector3 scale, Quaternion rotation)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
        }

        public override string ToString()
        {
            return "[Transform Position: " + Position + " Scale: " + Scale + " Rotation: " + Rotation + "]";
        }
    }
}
