using ProtoBufNet;

namespace NitroxServer.UnityStubs
{
    [ProtoContract]
    public class Quaternion
    {
        [ProtoMember(1)]
        public float x { get; }

        [ProtoMember(2)]
        public float y { get; }

        [ProtoMember(3)]
        public float z { get; }

        [ProtoMember(4)]
        public float w { get; }

        public Quaternion()
        {
        }

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Quaternion(UnityEngine.Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }

        public static implicit operator Quaternion(UnityEngine.Quaternion quaternion)
        {
            return new Quaternion(quaternion);
        }

        public static implicit operator UnityEngine.Quaternion(Quaternion quaternion)
        {
            return new UnityEngine.Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        public override string ToString()
        {
            return "[Quaternion - {" + x + ", " + y + ", " + z + "," + w + "}]";
        }
    }
}
