using ProtoBufNet;

namespace NitroxServer.UnityStubs
{
    [ProtoContract]
    public class Quaternion
    {
        [ProtoMember(1)]
        public float w { get; }

        [ProtoMember(2)]
        public float x { get; }

        [ProtoMember(3)]
        public float y { get; }

        [ProtoMember(4)]
        public float z { get; }

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

        public override string ToString()
        {
            return "[Quaternion - {" + x + ", " + y + ", " + z + "," + w + "}]";
        }
    }

}
