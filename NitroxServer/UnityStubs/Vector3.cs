using ProtoBufNet;

namespace NitroxServer.UnityStubs
{
    [ProtoContract]
    public class Vector3
    {
#pragma warning disable IDE1006 // Naming Styles
        [ProtoMember(1)]
        public float x { get; set; }

        [ProtoMember(2)]
        public float y { get; set; }

        [ProtoMember(3)]
        public float z { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public Vector3()
        {
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(UnityEngine.Vector3 unityVector)
        {
            x = unityVector.x;
            y = unityVector.y;
            z = unityVector.z;
        }

        public override string ToString()
        {
            return "[Vector3 - {" + x + ", " + y + ", " + z + "}]";
        }

        public static implicit operator Vector3(UnityEngine.Vector3 unityVector)
        {
            return new Vector3(unityVector);
        }

        public static implicit operator UnityEngine.Vector3(Vector3 vector)
        {
            return new UnityEngine.Vector3(vector.x, vector.y, vector.z);
        }
    }
}
