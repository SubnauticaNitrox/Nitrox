using System;
using NitroxModel.Helper;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [ProtoContract]
    [Serializable]
    public struct NitroxQuaternion
    {
        [ProtoMember(1)]
        public float X;

        [ProtoMember(2)]
        public float Y;

        [ProtoMember(3)]
        public float Z;

        [ProtoMember(4)]
        public float W;

        public NitroxQuaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static NitroxQuaternion Normalize(NitroxQuaternion value)
        {
            NitroxQuaternion ans;

            float ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z + value.W * value.W;

            float invNorm = 1.0f / (float)Math.Sqrt((double)ls);

            ans.X = value.X * invNorm;
            ans.Y = value.Y * invNorm;
            ans.Z = value.Z * invNorm;
            ans.W = value.W * invNorm;

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
                return quaternion;
            }
            if ((m00 >= m11) && (m00 >= m22))
            {
                float num7 = Mathf.Sqrt(((1f + m00) - m11) - m22);
                float num4 = 0.5f / num7;
                quaternion.X = 0.5f * num7;
                quaternion.Y = (m01 + m10) * num4;
                quaternion.Z = (m02 + m20) * num4;
                quaternion.W = (m12 - m21) * num4;
                return quaternion;
            }
            if (m11 > m22)
            {
                float num6 = Mathf.Sqrt(((1f + m11) - m00) - m22);
                float num3 = 0.5f / num6;
                quaternion.X = (m10 + m01) * num3;
                quaternion.Y = 0.5f * num6;
                quaternion.Z = (m21 + m12) * num3;
                quaternion.W = (m20 - m02) * num3;
                return quaternion;
            }
            float num5 = Mathf.Sqrt(((1f + m22) - m00) - m11);
            float num2 = 0.5f / num5;
            quaternion.X = (m20 + m02) * num2;
            quaternion.Y = (m21 + m12) * num2;
            quaternion.Z = 0.5f * num5;
            quaternion.W = (m01 - m10) * num2;
            return quaternion;
        }

        public NitroxVector3 ToEuler()
        {
            NitroxVector3 result;

            float test = X * Y + Z * W;
            // singularity at north pole
            if (test > 0.499)
            {
                result.X = 0;
                result.Y = 2 * Mathf.Atan2(X, W);
                result.Z = Mathf.PI / 2;
            }
            // singularity at south pole
            else if (test < -0.499)
            {
                result.X = 0;
                result.Y = -2 * Mathf.Atan2(X, W);
                result.Z = -Mathf.PI / 2;
            }
            else
            {
                result.X = Mathf.RAD2DEG * Mathf.Atan2(2 * X * W - 2 * Y * Z, 1 - 2 * X * X - 2 * Z * Z);
                result.Y = Mathf.RAD2DEG * Mathf.Atan2(2 * Y * W - 2 * X * Z, 1 - 2 * Y * Y - 2 * Z * Z);
                result.Z = Mathf.RAD2DEG * Mathf.Asin(2 * X * Y + 2 * Z * W);

                if (result.X < 0)
                    result.X += 360;
                if (result.Y < 0)
                    result.Y += 360;
                if (result.Z < 0)
                    result.Z += 360;
            }
            return result;
        }

        public NitroxQuaternion(UnityEngine.Quaternion quaternion)
        {
            X = quaternion.x;
            Y = quaternion.y;
            Z = quaternion.z;
            W = quaternion.w;
        }

        public static NitroxQuaternion operator *(NitroxQuaternion lhs, NitroxQuaternion rhs)
        {
            return new NitroxQuaternion(lhs.W * rhs.X + lhs.X * rhs.W + lhs.Y * rhs.Z - lhs.Z * rhs.Y,
                lhs.W * rhs.Y + lhs.Y * rhs.W + lhs.Z * rhs.X - lhs.X * rhs.Z,
                lhs.W * rhs.Z + lhs.Z * rhs.W + lhs.X * rhs.Y - lhs.Y * rhs.X,
                lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z);
        }

        public static implicit operator NitroxQuaternion(UnityEngine.Quaternion quaternion)
        {
            return new NitroxQuaternion(quaternion);
        }

        public static implicit operator UnityEngine.Quaternion(NitroxQuaternion quaternion)
        {
            return new UnityEngine.Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
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
    }
}
