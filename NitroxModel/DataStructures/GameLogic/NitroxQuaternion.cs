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

        public static NitroxQuaternion Identity { get; } = new NitroxQuaternion(0, 0, 0, 1);

        public NitroxQuaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        //From Unity
        public static NitroxQuaternion Normalize(NitroxQuaternion value)
        {
            float num = value.X * value.X + value.Y * value.Y + value.Z * value.Z + value.W * value.W;

            if (num < (double)float.Epsilon)
            {
                return Identity;
            }

            float sqrt = Mathf.Sqrt(num);
            return new NitroxQuaternion(value.X / sqrt, value.Y / sqrt, value.Z / sqrt, value.W / sqrt);
        }

        // From https://answers.unity.com/questions/467614/what-is-the-source-code-of-quaternionlookrotation.html
        // and  https://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/
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


            float tr = m00 + m11 + m22;
            NitroxQuaternion quaternion = new();
            if (tr > 0)
            {
                float s = Mathf.Sqrt(tr + 1.0f) * 2f; // S=4*qw 
                quaternion.W = 0.25f * s;
                quaternion.Y = (m21 - m12) / s;
                quaternion.X = (m02 - m20) / s;
                quaternion.Z = (m10 - m01) / s;
            }
            else if ((m00 > m11) & (m00 > m22))
            {
                float s = Mathf.Sqrt(1.0f + m00 - m11 - m22) * 2; // S=4*qx 
                quaternion.W = (m21 - m12) / s;
                quaternion.Y = 0.25f * s;
                quaternion.X = (m01 + m10) / s;
                quaternion.Z = (m02 + m20) / s;
            }
            else if (m11 > m22)
            {
                float s = Mathf.Sqrt(1.0f + m11 - m00 - m22) * 2; // S=4*qy
                quaternion.W = (m02 - m20) / s;
                quaternion.Y = (m01 + m10) / s;
                quaternion.X = 0.25f * s;
                quaternion.Z = (m12 + m21) / s;
            }
            else
            {
                float s = Mathf.Sqrt(1.0f + m22 - m00 - m11) * 2; // S=4*qz
                quaternion.W = (m10 - m01) / s;
                quaternion.Y = (m02 + m20) / s;
                quaternion.X = (m12 + m21) / s;
                quaternion.Z = 0.25f * s;
            }

            return Normalize(quaternion);
        }

        // From https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/index.htm
        public NitroxVector3 ToEuler()
        {
            float sqw = W * W;
            float sqx = X * X;
            float sqy = Y * Y;
            float sqz = Z * Z;
            float unit = sqx + sqy + sqz + sqw; // if normalized is one, otherwise is correction factor
            float test = X * W - Y * Z;
            NitroxVector3 v;

            if (test > 0.4995f * unit)
            { // singularity at north pole
                v.Y = 2f * Mathf.Atan2(Y, X);
                v.X = Mathf.PI / 2;
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
            v.Y = Mathf.Atan2(2f * q.X * q.W + 2f * q.Y * q.Z, 1 - 2f * (q.Z * q.Z + q.W * q.W));   // Yaw
            v.X = Mathf.Asin(2f * (q.X * q.Z - q.W * q.Y));                                         // Pitch
            v.Z = Mathf.Atan2(2f * q.X * q.Y + 2f * q.Z * q.W, 1 - 2f * (q.Y * q.Y + q.Z * q.Z));   // Roll
            return NormalizeAngles(v * Mathf.RAD2DEG);
        }

        // From https://www.euclideanspace.com/maths/geometry/rotations/conversions/eulerToQuaternion/index.htm
        public static NitroxQuaternion FromEuler(NitroxVector3 euler)
        {
            NitroxVector3 normEuler = NormalizeAngles(euler);
            NitroxVector3 radEuler = normEuler * ((float)Math.PI / 180f); //Yeah, Unity uses different formulas for setting and getting euler. (╯°□°)╯︵ ┻━┻
            NitroxQuaternion result;

            double yawOver2 = radEuler.Y * 0.5;
            double c1 = Math.Cos(yawOver2);
            double s1 = Math.Sin(yawOver2);
            double pitchOver2 = radEuler.X * 0.5;
            double c2 = Math.Cos(pitchOver2);
            double s2 = Math.Sin(pitchOver2);
            double rollOver2 = radEuler.Z * 0.5;
            double c3 = Math.Cos(rollOver2);
            double s3 = Math.Sin(rollOver2);

            result.W = (float)(c1 * c2 * c3 + s1 * s2 * s3);
            result.X = (float)(c1 * s2 * c3 + s1 * c2 * s3);
            result.Y = (float)(s1 * c2 * c3 - c1 * s2 * s3);
            result.Z = (float)(c1 * c2 * s3 - s1 * s2 * c3);

            return Normalize(result);
        }

        private static NitroxVector3 NormalizeAngles(NitroxVector3 angles)
        {
            return new NitroxVector3(NormalizeAngle(angles.X), NormalizeAngle(angles.Y), NormalizeAngle(angles.Z));
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle < 0)
            {
                angle += 360;
            }

            while (angle >= 360)
            {
                angle -= 360;
            }

            return angle;
        }

        public static NitroxQuaternion operator *(NitroxQuaternion lhs, NitroxQuaternion rhs)
        {
            return new NitroxQuaternion((float)(lhs.W * (double)rhs.X + lhs.X * (double)rhs.W + lhs.Y * (double)rhs.Z - lhs.Z * (double)rhs.Y),
                                        (float)(lhs.W * (double)rhs.Y + lhs.Y * (double)rhs.W + lhs.Z * (double)rhs.X - lhs.X * (double)rhs.Z),
                                        (float)(lhs.W * (double)rhs.Z + lhs.Z * (double)rhs.W + lhs.X * (double)rhs.Y - lhs.Y * (double)rhs.X),
                                        (float)(lhs.W * (double)rhs.W - lhs.X * (double)rhs.X - lhs.Y * (double)rhs.Y - lhs.Z * (double)rhs.Z));

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
            return $"[Quaternion - {X}, {Y}, {Z}, {W}]";
        }

        public override bool Equals(object obj)
        {
            return obj is NitroxQuaternion quaternion && Equals(quaternion);
        }

        public bool Equals(NitroxQuaternion other)
        {
            return Equals(other, float.Epsilon);
        }

        public bool Equals(NitroxQuaternion other, float tolerance)
        {
            return X == other.X && Y == other.Y && Z == other.Z && W == other.W ||
                   Math.Abs(other.X - X) < tolerance &&
                   Math.Abs(other.Y - Y) < tolerance &&
                   Math.Abs(other.Z - Z) < tolerance &&
                   Math.Abs(other.W - W) < tolerance;
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
