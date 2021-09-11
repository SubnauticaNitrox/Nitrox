using System;
using System.Numerics;
using NitroxModel.Helper;
using ProtoBufNet;

namespace NitroxModel.DataStructures.Unity
{
    [ProtoContract]
    [Serializable]
    public struct NitroxQuaternion : IEquatable<NitroxQuaternion>
    {
        [ProtoMember(1)]
        public float X;

        [ProtoMember(2)]
        public float Y;

        [ProtoMember(3)]
        public float Z;

        [ProtoMember(4)]
        public float W;

        public static NitroxQuaternion Identity { get; } = new NitroxQuaternion(0, 0, 0, 1);

        public NitroxQuaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static NitroxQuaternion Normalize(NitroxQuaternion value) => (NitroxQuaternion)Quaternion.Normalize((Quaternion)value);

        public static NitroxQuaternion FromEuler(NitroxVector3 vector) => FromEuler(vector.X, vector.Y, vector.Z);

        public static NitroxQuaternion FromEuler(float x, float y, float z) => (NitroxQuaternion)Quaternion.CreateFromYawPitchRoll(y * Mathf.DEG2RAD, x * Mathf.DEG2RAD, z * Mathf.DEG2RAD);

        //Used https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/
        public NitroxVector3 ToEuler()
        {
            float sqw = W * W;
            float sqx = X * X;
            float sqy = Y * Y;
            float sqz = Z * Z;

            float unit = sqx + sqy + sqz + sqw; // if normalized is one, otherwise is correction factor
            float test = X * Y + Z * W;

            float heading, attitude, bank;
            if (test > 0.499f * unit) // singularity at north pole
            {
                heading = 2 * Mathf.Atan2(X, W);
                attitude = Mathf.PI / 2f;
                bank = 0;
            }
            else if (test < -0.499f * unit) // singularity at south pole
            {
                heading = -2 * Mathf.Atan2(X, W);
                attitude = -Mathf.PI / 2f;
                bank = 0;
            }
            else
            {
                heading = Mathf.Atan2(2 * Y * W - 2 * X * Z, sqx - sqy - sqz + sqw);
                attitude = Mathf.Asin(2 * test / unit);
                bank = Mathf.Atan2(2 * X * W - 2 * Y * Z, -sqx + sqy - sqz + sqw);
            }

            NitroxVector3 euler = new NitroxVector3(bank * Mathf.RAD2DEG, heading * Mathf.RAD2DEG, attitude * Mathf.RAD2DEG);
            NormalizeEuler(ref euler);

            return euler;
        }

        public static void NormalizeEuler(ref NitroxVector3 vector3)
        {
            NormalizeEulerPart(ref vector3.X);
            NormalizeEulerPart(ref vector3.Y);
            NormalizeEulerPart(ref vector3.Z);
        }

        private static void NormalizeEulerPart(ref float euler)
        {
            while (euler > 360)
            {
                euler -= 360;
            }

            while (euler < 0)
            {
                euler += 360;
            }
        }

        public static NitroxQuaternion operator *(NitroxQuaternion lhs, NitroxQuaternion rhs) => (NitroxQuaternion)Quaternion.Multiply((Quaternion)lhs, (Quaternion)rhs);

        public static explicit operator Quaternion(NitroxQuaternion q) => new Quaternion(q.X, q.Y, q.Z, q.W);

        public static explicit operator NitroxQuaternion(Quaternion q) => new NitroxQuaternion(q.X, q.Y, q.Z, q.W);

        public static bool operator ==(NitroxQuaternion left, NitroxQuaternion right) => left.Equals(right);

        public static bool operator !=(NitroxQuaternion left, NitroxQuaternion right) => !left.Equals(right);

        public override bool Equals(object obj)
        {
            return obj is NitroxQuaternion other && Equals(other);
        }

        public bool Equals(NitroxQuaternion other)
        {
            return Equals(other, float.Epsilon);
        }

        public bool Equals(NitroxQuaternion other, float tolerance)
        {
            return X == other.X && Y == other.Y && Z == other.Z && W == other.W ||
                   Math.Abs(other.X + X) < tolerance && Math.Abs(other.Y + Y) < tolerance && Math.Abs(other.Z + Z) < tolerance && Math.Abs(other.W + W) < tolerance ||
                   Math.Abs(other.X - X) < tolerance && Math.Abs(other.Y - Y) < tolerance && Math.Abs(other.Z - Z) < tolerance && Math.Abs(other.W - W) < tolerance;
        }

        public override string ToString()
        {
            return $"[Quaternion: {X}, {Y}, {Z}, {W}]";
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ W.GetHashCode();
                return hashCode;
            }
        }
    }
}
