using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [ProtoContract]
    [Serializable]
    public class NitroxQuaternion
    {
        [ProtoMember(1)]
        public float x { get; }

        [ProtoMember(2)]
        public float y { get; }

        [ProtoMember(3)]
        public float z { get; }

        [ProtoMember(4)]
        public float w { get; } = 1.0f;

        public NitroxQuaternion()
        {
        }

        public NitroxQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public NitroxQuaternion(UnityEngine.Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }

        public static NitroxQuaternion operator *(NitroxQuaternion lhs, NitroxQuaternion rhs)
        {
            return new NitroxQuaternion(lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x,
                lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
        }

        public static implicit operator NitroxQuaternion(UnityEngine.Quaternion quaternion)
        {
            return new NitroxQuaternion(quaternion);
        }

        public static implicit operator UnityEngine.Quaternion(NitroxQuaternion quaternion)
        {
            return new UnityEngine.Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        public override string ToString()
        {
            return "[Quaternion - {" + x + ", " + y + ", " + z + "," + w + "}]";
        }
    }
}
