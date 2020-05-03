using System;
using NitroxModel.Helper;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    [ProtoContract]
    [Serializable]
    public struct Vector3
    {
        [ProtoMember(1)]
        public float X;

        [ProtoMember(2)]
        public float Y;

        [ProtoMember(3)]
        public float Z;

        public static Vector3 Zero { get; } = new Vector3(0, 0, 0);
        public static Vector3 One { get; } = new Vector3(1, 1, 1);

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X,
            a.Y + b.Y,
            a.Z + b.Z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X,
            a.Y - b.Y,
            a.Z - b.Z);
        }

        public static Vector3 operator -(Vector3 a)
        {
            return new Vector3(-a.X,
            -a.Y,
            -a.Z);
        }

        public static Vector3 operator /(Vector3 lhs, float rhs)
        {
            return new Vector3(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs);
        }

        public static Vector3 operator *(Vector3 lhs, float rhs)
        {
            return new Vector3(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);
        }

        public static Vector3 Normalize(Vector3 value)
        {
            float ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
            float length = Mathf.Sqrt(ls);
            return new Vector3(value.X / length, value.Y / length, value.Z / length);
        }

        public static float Length(Vector3 value)
        {
            float ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
            return Mathf.Sqrt(ls);
        }

        public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
        {
            return new Vector3(
                vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z,
                vector1.X * vector2.Y - vector1.Y * vector2.X);
        }
        
        public static implicit operator Vector3(Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public override string ToString()
        {
            return $"[{nameof(Vector3)} - {{{X}, {Y}, {Z}}}]";
        }
    }
}
