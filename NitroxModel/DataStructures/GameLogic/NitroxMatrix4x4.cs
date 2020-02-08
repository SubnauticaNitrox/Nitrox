using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.DataStructures.GameLogic
{
    public struct NitroxMatrix4x4
    {
        public float[,] M;

        private static readonly NitroxMatrix4x4 identity = new NitroxMatrix4x4
            (
            1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f
            );

        public float this[int x, int y]
        {
            get
            {
                return M[x, y];
            }
            set
            {
                M[x, y] = value;
            }
        }

        public NitroxMatrix4x4 Identity
        {
            get
            {
                return identity;
            }
        }

        public bool IsIdentity
        {
            get
            {
                return M[0, 0] == 1f && M[1, 1] == 1f && M[2, 2] == 1f && M[3, 3] == 1f &&
                    M[0, 1] == 0f && M[0, 2] == 0f && M[0, 3] == 0f &&
                    M[1, 0] == 0f && M[1, 2] == 0f && M[1, 3] == 0f &&
                    M[2, 0] == 0f && M[2, 1] == 0f && M[2, 3] == 0f &&
                    M[3, 0] == 0f && M[3, 1] == 0f && M[3, 2] == 0f;
            }
        }

        public NitroxMatrix4x4(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
        {
            M = new float[4, 4];
            M[0, 0] = m11;
            M[0, 1] = m12;
            M[0, 2] = m13;
            M[0, 3] = m14;
            M[1, 0] = m21;
            M[1, 1] = m22;
            M[1, 2] = m23;
            M[1, 3] = m24;
            M[2, 0] = m31;
            M[2, 1] = m32;
            M[2, 2] = m33;
            M[2, 3] = m34;
            M[3, 0] = m41;
            M[3, 1] = m42;
            M[3, 2] = m43;
            M[3, 3] = m44;
        }

        public static NitroxMatrix4x4 SetScale(NitroxVector3 localScale)
        {
            NitroxMatrix4x4 scaleMatrix;
            scaleMatrix.M = new float[4, 4];
            scaleMatrix[0, 0] = 1f;
            scaleMatrix[1, 1] = 1f;
            scaleMatrix[2, 2] = 1f;
            scaleMatrix[3, 3] = 1f;


            scaleMatrix[0, 0] = localScale.X;
            scaleMatrix[1, 1] = localScale.Y;
            scaleMatrix[2, 2] = localScale.Z;

            return scaleMatrix;
        }

        public float GetDeterminant()
        {
            // | a b c d |     | f g h |     | e g h |     | e f h |     | e f g |
            // | e f g h | = a | j k l | - b | i k l | + c | i j l | - d | i j k |
            // | i j k l |     | n o p |     | m o p |     | m n p |     | m n o |
            // | m n o p |
            //
            //   | f g h |
            // a | j k l | = a ( f ( kp - lo ) - g ( jp - ln ) + h ( jo - kn ) )
            //   | n o p |
            //
            //   | e g h |     
            // b | i k l | = b ( e ( kp - lo ) - g ( ip - lm ) + h ( io - km ) )
            //   | m o p |     
            //
            //   | e f h |
            // c | i j l | = c ( e ( jp - ln ) - f ( ip - lm ) + h ( in - jm ) )
            //   | m n p |
            //
            //   | e f g |
            // d | i j k | = d ( e ( jo - kn ) - f ( io - km ) + g ( in - jm ) )
            //   | m n o |
            //
            // Cost of operation
            // 17 adds and 28 muls.
            //
            // add: 6 + 8 + 3 = 17
            // mul: 12 + 16 = 28

            float a = M[0,0], b = M[0, 1], c = M[0, 2], d = M[0, 3];
            float e = M[1, 0], f = M[1, 1], g = M[1, 2], h = M[1, 3];
            float i = M[2, 0], j = M[2, 1], k = M[2, 2], l = M[2, 3];
            float m = M[3, 0], n = M[3, 1], o = M[3, 2], p = M[3, 3];

            float kp_lo = k * p - l * o;
            float jp_ln = j * p - l * n;
            float jo_kn = j * o - k * n;
            float ip_lm = i * p - l * m;
            float io_km = i * o - k * m;
            float in_jm = i * n - j * m;

            return a * (f * kp_lo - g * jp_ln + h * jo_kn) -
                   b * (e * kp_lo - g * ip_lm + h * io_km) +
                   c * (e * jp_ln - f * ip_lm + h * in_jm) -
                   d * (e * jo_kn - f * io_km + g * in_jm);
        }

        public static NitroxMatrix4x4 SetRotation(NitroxQuaternion localRotation)
        {
            NitroxMatrix4x4 rotationMatrix;
            rotationMatrix.M = new float[4, 4];
            rotationMatrix[3, 3] = 1f;

            float xx = localRotation.X * localRotation.X;
            float yy = localRotation.Y * localRotation.Y;
            float zz = localRotation.Z * localRotation.Z;

            float xy = localRotation.X * localRotation.Y;
            float wz = localRotation.W * localRotation.Z;
            float xz = localRotation.X * localRotation.Z;
            float wy = localRotation.W * localRotation.Y;
            float yz = localRotation.Y * localRotation.Z;
            float wx = localRotation.W * localRotation.X;

            rotationMatrix[0, 0] = 1f - 2f * (yy + zz);
            rotationMatrix[0, 1] = 2f * (xy + wz);
            rotationMatrix[0, 2] = 2f * (xz - wy);

            rotationMatrix[1, 0] = 2f * (xy - wz);
            rotationMatrix[1, 1] = 1f - 2f * (zz + xx);
            rotationMatrix[1, 2] = 2f * (yz + wy);

            rotationMatrix[2, 0] = 2f * (xz - wy);
            rotationMatrix[2, 1] = 2f * (yz + wx);
            rotationMatrix[2, 2] = 1f - 2f * (yz - wy);

            return rotationMatrix;
        }

        public static NitroxMatrix4x4 SetTranslation(NitroxVector3 localPosition)
        {
            NitroxMatrix4x4 transMatrix;
            transMatrix.M = new float[4, 4];
            transMatrix[0, 0] = 1f;
            transMatrix[1, 1] = 1f;
            transMatrix[2, 2] = 1f;
            transMatrix[3, 3] = 1f;



            transMatrix[0, 3] = localPosition.X;
            transMatrix[1, 3] = localPosition.Y;
            transMatrix[2, 3] = localPosition.Z;

            return transMatrix;
        }

        public static NitroxMatrix4x4 operator *(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result;
            result.M = new float[4, 4]; // 4x4 array
            result[0, 0] = lhs[0, 0] * rhs[0, 0] + lhs[0, 1] * rhs[1, 0] + lhs[0, 2] * rhs[2, 0] + lhs[0, 3] * rhs[3, 0];
            result[0, 1] = lhs[0, 0] * rhs[0, 1] + lhs[0, 1] * rhs[1, 1] + lhs[0, 2] * rhs[2, 1] + lhs[0, 3] * rhs[3, 1];
            result[0, 2] = lhs[0, 0] * rhs[0, 2] + lhs[0, 1] * rhs[1, 2] + lhs[0, 2] * rhs[2, 2] + lhs[0, 3] * rhs[3, 2];
            result[0, 3] = lhs[0, 0] * rhs[0, 3] + lhs[0, 1] * rhs[1, 3] + lhs[0, 2] * rhs[2, 3] + lhs[0, 3] * rhs[3, 3];

            result[1, 0] = lhs[1, 0] * rhs[0, 0] + lhs[1, 1] * rhs[1, 0] + lhs[1, 2] * rhs[2, 0] + lhs[1, 3] * rhs[3, 0];
            result[1, 1] = lhs[1, 0] * rhs[0, 1] + lhs[1, 1] * rhs[1, 1] + lhs[1, 2] * rhs[2, 1] + lhs[1, 3] * rhs[3, 1];
            result[1, 2] = lhs[1, 0] * rhs[0, 2] + lhs[1, 1] * rhs[1, 2] + lhs[1, 2] * rhs[2, 2] + lhs[1, 3] * rhs[3, 2];
            result[1, 3] = lhs[1, 0] * rhs[0, 3] + lhs[1, 1] * rhs[1, 3] + lhs[1, 2] * rhs[2, 3] + lhs[1, 3] * rhs[3, 3];

            result[2, 0] = lhs[2, 0] * rhs[0, 0] + lhs[2, 1] * rhs[1, 0] + lhs[2, 2] * rhs[2, 0] + lhs[2, 3] * rhs[3, 0];
            result[2, 1] = lhs[2, 0] * rhs[0, 1] + lhs[2, 1] * rhs[1, 1] + lhs[2, 2] * rhs[2, 1] + lhs[2, 3] * rhs[3, 1];
            result[2, 2] = lhs[2, 0] * rhs[0, 2] + lhs[2, 1] * rhs[1, 2] + lhs[2, 2] * rhs[2, 2] + lhs[2, 3] * rhs[3, 2];
            result[2, 3] = lhs[2, 0] * rhs[0, 3] + lhs[2, 1] * rhs[1, 3] + lhs[2, 2] * rhs[2, 3] + lhs[2, 3] * rhs[3, 3];

            result[3, 0] = lhs[3, 0] * rhs[0, 0] + lhs[3, 1] * rhs[1, 0] + lhs[3, 2] * rhs[2, 0] + lhs[3, 3] * rhs[3, 0];
            result[3, 1] = lhs[3, 0] * rhs[0, 1] + lhs[3, 1] * rhs[1, 1] + lhs[3, 2] * rhs[2, 1] + lhs[3, 3] * rhs[3, 1];
            result[3, 2] = lhs[3, 0] * rhs[0, 2] + lhs[3, 1] * rhs[1, 2] + lhs[3, 2] * rhs[2, 2] + lhs[3, 3] * rhs[3, 2];
            result[3, 3] = lhs[3, 0] * rhs[0, 3] + lhs[3, 1] * rhs[1, 3] + lhs[3, 2] * rhs[2, 3] + lhs[3, 3] * rhs[3, 3];

            return result;
        }

        public static NitroxMatrix4x4 operator +(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result;
            result.M = new float[4, 4];
            
            for (int i = 0; i < 4; i++)
            {
                result[i, i] = lhs[i, i] + rhs[i, i];
            }


            return result;
        }
        public static NitroxMatrix4x4 operator -(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result;
            result.M = new float[4, 4];

            for (int i = 0; i < 4; i++)
            {
                result[i, i] = lhs[i, i] - rhs[i, i];
            }


            return result;
        }

        public static NitroxMatrix4x4 TRS(NitroxVector3 localPos, NitroxQuaternion localRotation, NitroxVector3 localScale)
        {
            NitroxMatrix4x4 scaleMatrix = SetScale(localScale);
            NitroxMatrix4x4 rotationMatrix = SetRotation(localRotation);
            NitroxMatrix4x4 translationMatrix = SetTranslation(localPos);
            NitroxMatrix4x4 result = translationMatrix * rotationMatrix * scaleMatrix;
            return result;
        }

        public static NitroxVector3 ExtractScale(NitroxMatrix4x4 matrix)
        {
            NitroxVector3 scale;
            scale.X = NitroxVector3.Length(new NitroxVector3(matrix[0, 0], matrix[0, 1], matrix[0, 2]));
            scale.Y = NitroxVector3.Length(new NitroxVector3(matrix[1, 0], matrix[1, 1], matrix[1, 2]));
            scale.Z = NitroxVector3.Length(new NitroxVector3(matrix[2, 0], matrix[2, 1], matrix[2, 2]));

            matrix[0, 0] = -matrix[0, 0];
            matrix[0, 1] = -matrix[0, 1];
            matrix[0, 2] = -matrix[0, 2];

            matrix[1, 0] = -matrix[1, 0];
            matrix[1, 1] = -matrix[1, 1];
            matrix[1, 2] = -matrix[1, 2];

            matrix[2, 0] = -matrix[2, 0];
            matrix[2, 1] = -matrix[2, 1];
            matrix[2, 2] = -matrix[2, 2];

            return scale;
        }

        public static NitroxVector3 ExtractTranslation(NitroxMatrix4x4 matrix)
        {
            NitroxVector3 position;
            position.X = matrix[0,3];
            position.Y = matrix[1,3];
            position.Z = matrix[2,3];
            return position;
        }

        public static NitroxQuaternion ExtractRotation(NitroxMatrix4x4 matrix)
        {
            NitroxVector3 forward;
            forward.X = matrix[0,2];
            forward.Y = matrix[1,2];
            forward.Z = matrix[2,2];

            NitroxVector3 upwards;
            upwards.X = matrix[0,1];
            upwards.Y = matrix[1,1];
            upwards.Z = matrix[2,1];

            return NitroxQuaternion.LookRotation(forward, upwards);
        }

        public static void DecomposeMatrix(ref NitroxMatrix4x4 matrix, out NitroxVector3 localPosition, out NitroxQuaternion localRotation, out NitroxVector3 localScale)
        {
            localPosition = ExtractTranslation(matrix);
            localScale = ExtractScale(matrix);
            localRotation = ExtractRotation(matrix);
        }
    }
}
