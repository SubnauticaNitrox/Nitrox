using System;
using ProtoBufNet;
using NitroxModel.Helper;

namespace NitroxModel.DataStructures.GameLogic
{
    [ProtoContract]
    [Serializable]
    public struct NitroxVector3
    {
        [ProtoMember(1)]
        public float X;

        [ProtoMember(2)]
        public float Y;

        [ProtoMember(3)]
        public float Z;

        public static NitroxVector3 Zero = new NitroxVector3(0, 0, 0);

        public NitroxVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public NitroxVector3(UnityEngine.Vector3 unityVector)
        {
            X = unityVector.x;
            Y = unityVector.y;
            Z = unityVector.z;
        }

        public static NitroxVector3 operator +(NitroxVector3 a, NitroxVector3 b)
        {
            return new NitroxVector3(a.X + b.X,
            a.Y + b.Y,
            a.Z + b.Z);
        }

        public static NitroxVector3 operator -(NitroxVector3 a, NitroxVector3 b)
        {
            return new NitroxVector3(a.X - b.X,
            a.Y - b.Y,
            a.Z - b.Z);
        }

        public static NitroxVector3 operator -(NitroxVector3 a)
        {
            return new NitroxVector3(-a.X,
            -a.Y,
            -a.Z);
        }

        public static NitroxVector3 operator /(NitroxVector3 lhs, float rhs)
        {
            return new NitroxVector3(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs);
        }

        public static NitroxVector3 Normalize(NitroxVector3 value)
        {
            float ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
            float length = Mathf.Sqrt(ls);
            return new NitroxVector3(value.X / length, value.Y / length, value.Z / length);
        }

        public static float Length(NitroxVector3 value)
        {
            float ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
            return Mathf.Sqrt(ls);
        }

        public static NitroxVector3 Cross(NitroxVector3 vector1, NitroxVector3 vector2)
        {
            return new NitroxVector3(
                vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z,
                vector1.X * vector2.Y - vector1.Y * vector2.X);
        }

        public override string ToString()
        {
            return "[Vector3 - {" + X + ", " + Y + ", " + Z + "}]";
        }

        public static implicit operator NitroxVector3(UnityEngine.Vector3 unityVector)
        {
            return new NitroxVector3(unityVector);
        }

        public static implicit operator UnityEngine.Vector3(NitroxVector3 vector)
        {
            return new UnityEngine.Vector3(vector.X, vector.Y, vector.Z);
        }
    }
}
