using System;
using NitroxModel.Helper;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
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

        public NitroxVector3 Euler { get { return ToEuler(); } }

        public static NitroxQuaternion Identity { get; } = new NitroxQuaternion(0, 0, 0, 1);

        public NitroxQuaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static NitroxQuaternion Normalize(NitroxQuaternion value)
        {
            NitroxQuaternion ans = value;

            float ls = ans.X * ans.X + ans.Y * ans.Y + ans.Z * ans.Z + ans.W * ans.W;
            float invNorm = 1.0f / (float)Math.Sqrt(ls);

            ans.X *= invNorm;
            ans.Y *= invNorm;
            ans.Z *= invNorm;
            ans.W *= invNorm;

            return ans;
        }

        public static NitroxQuaternion LookRotation(NitroxVector3 forward, NitroxVector3 up)
        {
            NitroxVector3 vector = NitroxVector3.Normalize(forward);
            NitroxVector3 vector2 = NitroxVector3.Normalize(NitroxVector3.Cross(up, vector));
            NitroxVector3 vector3 = NitroxVector3.Cross(vector, vector2);
            float m00 = vector2.X;
            float m01 = vector2.Y;
            float m02 = vector2.Z;
            float m10 = vector3.X;
            float m11 = vector3.Y;
            float m12 = vector3.Z;
            float m20 = vector.X;
            float m21 = vector.Y;
            float m22 = vector.Z;


            float num8 = (m00 + m11) + m22;
            NitroxQuaternion quaternion;
            if (num8 > 0f)
            {
                float num = Mathf.Sqrt(num8 + 1f);
                quaternion.W = num * 0.5f;
                num = 0.5f / num;
                quaternion.X = (m12 - m21) * num;
                quaternion.Y = (m20 - m02) * num;
                quaternion.Z = (m01 - m10) * num;
                return Normalize(quaternion);
            }
            if ((m00 >= m11) && (m00 >= m22))
            {
                float num7 = Mathf.Sqrt(((1f + m00) - m11) - m22);
                float num4 = 0.5f / num7;
                quaternion.X = 0.5f * num7;
                quaternion.Y = (m01 + m10) * num4;
                quaternion.Z = (m02 + m20) * num4;
                quaternion.W = (m12 - m21) * num4;
                return Normalize(quaternion);
            }
            if (m11 > m22)
            {
                float num6 = Mathf.Sqrt(((1f + m11) - m00) - m22);
                float num3 = 0.5f / num6;
                quaternion.X = (m10 + m01) * num3;
                quaternion.Y = 0.5f * num6;
                quaternion.Z = (m21 + m12) * num3;
                quaternion.W = (m20 - m02) * num3;
                return Normalize(quaternion);
            }
            float num5 = Mathf.Sqrt(((1f + m22) - m00) - m11);
            float num2 = 0.5f / num5;
            quaternion.X = (m20 + m02) * num2;
            quaternion.Y = (m21 + m12) * num2;
            quaternion.Z = 0.5f * num5;
            quaternion.W = (m01 - m10) * num2;

            return Normalize(quaternion);
        }

        public NitroxVector3 ToEuler()
        {
            float sqw = W * W;
            float sqx = X * X;
            float sqy = Y * Y;
            float sqz = Z * Z;
            float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float test = X * W - Y * Z;
            NitroxVector3 v;

            if (test > 0.4995f * unit)
            { // singularity at north pole
                v.X = 2f * Mathf.Atan2(Y, X);
                v.Y = Mathf.PI / 2;
                v.Z = 0;
                return NormalizeAngles(v * Mathf.RAD2DEG);
            }
            if (test < -0.4995f * unit)
            { // singularity at south pole
                v.X = -2f * Mathf.Atan2(Y, X);
                v.Y = -Mathf.PI / 2;
                v.Z = 0;
                return NormalizeAngles(v * Mathf.RAD2DEG);
            }
            NitroxQuaternion q = new NitroxQuaternion(W, Z, X, Y);
            v.X = Mathf.Atan2(2f * q.X * q.W + 2f * q.Y * q.Z, 1 - 2f * (q.Z * q.Z + q.W * q.W));     // Yaw
            v.Y = Mathf.Asin(2f * (q.X * q.Z - q.W * q.Y));                             // Pitch
            v.Z = Mathf.Atan2(2f * q.X * q.Y + 2f * q.Z * q.W, 1 - 2f * (q.Y * q.Y + q.Z * q.Z));      // Roll
            return NormalizeAngles(v * Mathf.RAD2DEG);
        }

        private static NitroxVector3 NormalizeAngles(NitroxVector3 angles)
        {
            angles.X = NormalizeAngle(angles.X);
            angles.Y = NormalizeAngle(angles.Y);
            angles.Z = NormalizeAngle(angles.Z);
            return angles;
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle >= 360)
            {
                angle -= 360;
            }

            while (angle < 0)
            {
                angle += 360;
            }
            return angle;
        }

        public static NitroxQuaternion FromEuler(NitroxVector3 euler)
        {
            NitroxVector3 normEuler = NormalizeAngles(euler);
            NitroxVector3 radEuler = normEuler * Mathf.DEG2RAD;
            NitroxQuaternion result;

            double yawOver2 = radEuler.Y * 0.5f;
            float cosYawOver2 = (float)System.Math.Cos(yawOver2);
            float sinYawOver2 = (float)System.Math.Sin(yawOver2);
            double pitchOver2 = radEuler.X * 0.5f;
            float cosPitchOver2 = (float)System.Math.Cos(pitchOver2);
            float sinPitchOver2 = (float)System.Math.Sin(pitchOver2);
            double rollOver2 = radEuler.Z * 0.5f;
            float cosRollOver2 = (float)System.Math.Cos(rollOver2);
            float sinRollOver2 = (float)System.Math.Sin(rollOver2);

            result.W = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
            result.X = sinYawOver2 * cosPitchOver2 * cosRollOver2 + cosYawOver2 * sinPitchOver2 * sinRollOver2;
            result.Y = cosYawOver2 * sinPitchOver2 * cosRollOver2 - sinYawOver2 * cosPitchOver2 * sinRollOver2;
            result.Z = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;

            return Normalize(result);
        }

        public static NitroxQuaternion operator *(NitroxQuaternion lhs, NitroxQuaternion rhs)
        {
            NitroxQuaternion result = new NitroxQuaternion(lhs.W * rhs.X + lhs.X * rhs.W + lhs.Y * rhs.Z - lhs.Z * rhs.Y,
                lhs.W * rhs.Y + lhs.Y * rhs.W + lhs.Z * rhs.X - lhs.X * rhs.Z,
                lhs.W * rhs.Z + lhs.Z * rhs.W + lhs.X * rhs.Y - lhs.Y * rhs.X,
                lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z);

            return result;
        }

        public static bool operator ==(NitroxQuaternion left, NitroxQuaternion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NitroxQuaternion left, NitroxQuaternion right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return "[Quaternion - {" + X + ", " + Y + ", " + Z + "," + W + "}]";
        }

        public static NitroxQuaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            //  Roll first, about axis the object is facing, then
            //  pitch upward, then yaw to face into the new heading
            float sr, cr, sp, cp, sy, cy;

            float halfRoll = roll * 0.5f;
            sr = (float)Math.Sin(halfRoll);
            cr = (float)Math.Cos(halfRoll);

            float halfPitch = pitch * 0.5f;
            sp = (float)Math.Sin(halfPitch);
            cp = (float)Math.Cos(halfPitch);

            float halfYaw = yaw * 0.5f;
            sy = (float)Math.Sin(halfYaw);
            cy = (float)Math.Cos(halfYaw);

            NitroxQuaternion result = new NitroxQuaternion();

            result.X = cy * sp * cr + sy * cp * sr;
            result.Y = sy * cp * cr - cy * sp * sr;
            result.Z = cy * cp * sr - sy * sp * cr;
            result.W = cy * cp * cr + sy * sp * sr;

            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is NitroxQuaternion quaternion && Equals(quaternion);
        }

        public bool Equals(NitroxQuaternion other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z &&
                   W == other.W;
        }

        public bool Equals(NitroxQuaternion other, float tolerance)
        {
            return (X == other.X || other.X >= X - tolerance && other.X <= X + tolerance) &&
                (Y == other.Y || other.Y >= Y - tolerance && other.Y <= Y + tolerance) &&
                (Z == other.Z || other.Z >= Z - tolerance && other.Z <= Z + tolerance) &&
                (W == other.W || other.W >= W - tolerance && other.W <= W + tolerance);
        }

        public override int GetHashCode()
        {
            int hashCode = 707706286;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            hashCode = hashCode * -1521134295 + W.GetHashCode();
            return hashCode;
        }
    }
}
