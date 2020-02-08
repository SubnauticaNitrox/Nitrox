using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [ProtoContract]
    [Serializable]
    public class NitroxVector3
    {
#pragma warning disable IDE1006 // Naming Styles
        [ProtoMember(1)]
        public float x { get; set; }

        [ProtoMember(2)]
        public float y { get; set; }

        [ProtoMember(3)]
        public float z { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public NitroxVector3()
        {
        }

        public NitroxVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public NitroxVector3(UnityEngine.Vector3 unityVector)
        {
            x = unityVector.x;
            y = unityVector.y;
            z = unityVector.z;
        }

        public static NitroxVector3 operator +(NitroxVector3 a, NitroxVector3 b)
        {
            return new NitroxVector3(a.x + b.x,
            a.y + b.y,
            a.z + b.z);
        }

        public override string ToString()
        {
            return "[Vector3 - {" + x + ", " + y + ", " + z + "}]";
        }

        public static implicit operator NitroxVector3(UnityEngine.Vector3 unityVector)
        {
            return new NitroxVector3(unityVector);
        }

        public static implicit operator UnityEngine.Vector3(NitroxVector3 vector)
        {
            return new UnityEngine.Vector3(vector.x, vector.y, vector.z);
        }
    }
}
