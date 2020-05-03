using System;
using NitroxModel.Helper;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    [ProtoContract]
    [Serializable]
    public struct Vector4
    {
        [ProtoMember(1)]
        public float X;

        [ProtoMember(2)]
        public float Y;

        [ProtoMember(3)]
        public float Z;

        [ProtoMember(4)]
        public float W;

        public static Vector4 Zero { get; } = new Vector4(0, 0, 0, 0);
        public static Vector4 One { get; } = new Vector4(1, 1, 1, 1);

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static Vector4 operator +(Vector4 a, Vector4 b)
        {
            return new Vector4(a.X + b.X,
                               a.Y + b.Y,
                               a.Z + b.Z,
                               a.W + b.W);
        }

        public static Vector4 operator -(Vector4 a, Vector4 b)
        {
            return new Vector4(a.X - b.X,
                               a.Y - b.Y,
                               a.Z - b.Z,
                               a.W - b.W);
        }

        public static Vector4 operator -(Vector4 a)
        {
            return new Vector4(-a.X,
                               -a.Y,
                               -a.Z,
                               -a.W);
        }

        public static Vector4 operator /(Vector4 lhs, float rhs)
        {
            return new Vector4(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs, lhs.W / rhs);
        }

        public static Vector4 operator *(Vector4 lhs, float rhs)
        {
            return new Vector4(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs, lhs.W * rhs);
        }

        public static Vector4 Normalize(Vector4 value)
        {
            float ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
            float length = Mathf.Sqrt(ls);
            return new Vector4(value.X / length, value.Y / length, value.Z / length, value.W / length);
        }

        public static float Length(Vector4 value)
        {
            float ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z + value.W * value.W;
            return Mathf.Sqrt(ls);
        }

        public static implicit operator Vector4(Color color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }

        public override string ToString()
        {
            return $"[{nameof(Vector4)} - {{{X}, {Y}, {Z}, {W}}}]";
        }
    }
}
