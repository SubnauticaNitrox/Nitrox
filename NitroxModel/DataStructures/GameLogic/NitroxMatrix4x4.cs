using System;

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

        public override string ToString()
        {
            return $"[\n[{M[0,0]}], [{M[0,1]}], [{M[0,2]}], [{M[0,3]}],\n" +
                $"[{M[1, 0]}], [{M[1, 1]}], [{M[1, 2]}], [{M[1, 3]}],\n" +
                $"[{M[2, 0]}], [{M[2, 1]}], [{M[2, 2]}], [{M[2, 3]}],\n" +
                $"[{M[3, 0]}], [{M[3, 1]}], [{M[3, 2]}], [{M[3, 3]}],\n]";
        }

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

        public static NitroxMatrix4x4 Identity
        {
            get
            {
                return identity;
            }
        }

        public NitroxMatrix4x4 Inverse
        {
            get
            {
                return Invert(this);
            }
        }

        public static NitroxMatrix4x4 Invert(NitroxMatrix4x4 matrix)
        {
            NitroxMatrix4x4 result;
            result.M = new float[4, 4];

            float a = matrix[0,0], b = matrix[0,1], c = matrix[0,2], d = matrix[0,3];
            float e = matrix[1,0], f = matrix[1,1], g = matrix[1,2], h = matrix[1,3];
            float i = matrix[2,0], j = matrix[2,1], k = matrix[2,2], l = matrix[2,3];
            float m = matrix[3,0], n = matrix[3,1], o = matrix[3,2], p = matrix[3,3];

            float kp_lo = k * p - l * o;
            float jp_ln = j * p - l * n;
            float jo_kn = j * o - k * n;
            float ip_lm = i * p - l * m;
            float io_km = i * o - k * m;
            float in_jm = i * n - j * m;

            float a11 = +(f * kp_lo - g * jp_ln + h * jo_kn);
            float a12 = -(e * kp_lo - g * ip_lm + h * io_km);
            float a13 = +(e * jp_ln - f * ip_lm + h * in_jm);
            float a14 = -(e * jo_kn - f * io_km + g * in_jm);

            float det = a * a11 + b * a12 + c * a13 + d * a14;

            float invDet = 1.0f / det;

            result[0,0] = a11 * invDet;
            result[1,0] = a12 * invDet;
            result[2,0] = a13 * invDet;
            result[3,0] = a14 * invDet;

            result[0,1] = -(b * kp_lo - c * jp_ln + d * jo_kn) * invDet;
            result[1,1] = +(a * kp_lo - c * ip_lm + d * io_km) * invDet;
            result[2,1] = -(a * jp_ln - b * ip_lm + d * in_jm) * invDet;
            result[3,1] = +(a * jo_kn - b * io_km + c * in_jm) * invDet;

            float gp_ho = g * p - h * o;
            float fp_hn = f * p - h * n;
            float fo_gn = f * o - g * n;
            float ep_hm = e * p - h * m;
            float eo_gm = e * o - g * m;
            float en_fm = e * n - f * m;

            result[0,2] = +(b * gp_ho - c * fp_hn + d * fo_gn) * invDet;
            result[1,2] = -(a * gp_ho - c * ep_hm + d * eo_gm) * invDet;
            result[2,2] = +(a * fp_hn - b * ep_hm + d * en_fm) * invDet;
            result[3,2] = -(a * fo_gn - b * eo_gm + c * en_fm) * invDet;

            float gl_hk = g * l - h * k;
            float fl_hj = f * l - h * j;
            float fk_gj = f * k - g * j;
            float el_hi = e * l - h * i;
            float ek_gi = e * k - g * i;
            float ej_fi = e * j - f * i;

            result[0,3] = -(b * gl_hk - c * fl_hj + d * fk_gj) * invDet;
            result[1,3] = +(a * gl_hk - c * el_hi + d * ek_gi) * invDet;
            result[2,3] = -(a * fl_hj - b * el_hi + d * ej_fi) * invDet;
            result[3,3] = +(a * fk_gj - b * ek_gi + c * ej_fi) * invDet;

            return result;
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

        public NitroxMatrix4x4(NitroxMatrix4x4 matrix)
        {
            M = new float[4, 4];
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    M[x, y] = matrix[x, y];
                }
            }
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
            NitroxQuaternion rot = NitroxQuaternion.Normalize(localRotation);
            NitroxMatrix4x4 rotationMatrix;
            rotationMatrix.M = new float[4, 4];
            rotationMatrix[3, 3] = 1f;

            float sqw = rot.W * rot.W;
            float sqx = rot.X * rot.X;
            float sqy = rot.Y * rot.Y;
            float sqz = rot.Z * rot.Z;

            float invs = 1 / (sqx + sqy + sqz + sqw);
            rotationMatrix[0, 0] = (sqx - sqy - sqz + sqw) * invs;
            rotationMatrix[1, 1] = (-sqx + sqy - sqz + sqw) * invs;
            rotationMatrix[2, 2] = (-sqx - sqy + sqz + sqw) * invs;

            float tmp1 = rot.X * rot.Y;
            float tmp2 = rot.Z * rot.W;

            rotationMatrix[1, 0] = 2 * (tmp1 + tmp2) * invs;
            rotationMatrix[0, 1] = 2 * (tmp1 - tmp2) * invs;

            tmp1 = rot.X * rot.Z;
            tmp2 = rot.Y * rot.W;

            rotationMatrix[2, 0] = 2 * (tmp1 - tmp2) * invs;
            rotationMatrix[0, 2] = 2 * (tmp1 + tmp2) * invs;

            tmp1 = rot.Y * rot.Z;
            tmp2 = rot.X * rot.W;

            rotationMatrix[2, 1] = 2 * (tmp1 + tmp2) * invs;
            rotationMatrix[1, 2] = 2 * (tmp1 - tmp2) * invs;

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

        public static NitroxVector3 ExtractTranslation(ref NitroxMatrix4x4 matrix)
        {
            NitroxVector3 position;
            position.X = matrix[0, 3];
            position.Y = matrix[1, 3];
            position.Z = matrix[2, 3];
            return position;
        }

        public static NitroxMatrix4x4 operator *(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result;
            result.M = new float[4, 4]; // 4x4 array

            // First row
            result[0,0] = lhs[0,0] * rhs[0,0] + lhs[0,1] * rhs[1,0] + lhs[0,2] * rhs[2,0] + lhs[0,3] * rhs[3,0];
            result[0,1] = lhs[0,0] * rhs[0,1] + lhs[0,1] * rhs[1,1] + lhs[0,2] * rhs[2,1] + lhs[0,3] * rhs[3,1];
            result[0,2] = lhs[0,0] * rhs[0,2] + lhs[0,1] * rhs[1,2] + lhs[0,2] * rhs[2,2] + lhs[0,3] * rhs[3,2];
            result[0,3] = lhs[0,0] * rhs[0,3] + lhs[0,1] * rhs[1,3] + lhs[0,2] * rhs[2,3] + lhs[0,3] * rhs[3,3];

            // Second row
            result[1,0] = lhs[1,0] * rhs[0,0] + lhs[1,1] * rhs[1,0] + lhs[1,2] * rhs[2,0] + lhs[1,3] * rhs[3,0];
            result[1,1] = lhs[1,0] * rhs[0,1] + lhs[1,1] * rhs[1,1] + lhs[1,2] * rhs[2,1] + lhs[1,3] * rhs[3,1];
            result[1,2] = lhs[1,0] * rhs[0,2] + lhs[1,1] * rhs[1,2] + lhs[1,2] * rhs[2,2] + lhs[1,3] * rhs[3,2];
            result[1,3] = lhs[1,0] * rhs[0,3] + lhs[1,1] * rhs[1,3] + lhs[1,2] * rhs[2,3] + lhs[1,3] * rhs[3,3];

            // Third row
            result[2,0] = lhs[2,0] * rhs[0,0] + lhs[2,1] * rhs[1,0] + lhs[2,2] * rhs[2,0] + lhs[2,3] * rhs[3,0];
            result[2,1] = lhs[2,0] * rhs[0,1] + lhs[2,1] * rhs[1,1] + lhs[2,2] * rhs[2,1] + lhs[2,3] * rhs[3,1];
            result[2,2] = lhs[2,0] * rhs[0,2] + lhs[2,1] * rhs[1,2] + lhs[2,2] * rhs[2,2] + lhs[2,3] * rhs[3,2];
            result[2,3] = lhs[2,0] * rhs[0,3] + lhs[2,1] * rhs[1,3] + lhs[2,2] * rhs[2,3] + lhs[2,3] * rhs[3,3];

            // Fourth row
            result[3,0] = lhs[3,0] * rhs[0,0] + lhs[3,1] * rhs[1,0] + lhs[3,2] * rhs[2,0] + lhs[3,3] * rhs[3,0];
            result[3,1] = lhs[3,0] * rhs[0,1] + lhs[3,1] * rhs[1,1] + lhs[3,2] * rhs[2,1] + lhs[3,3] * rhs[3,1];
            result[3,2] = lhs[3,0] * rhs[0,2] + lhs[3,1] * rhs[1,2] + lhs[3,2] * rhs[2,2] + lhs[3,3] * rhs[3,2];
            result[3,3] = lhs[3,0] * rhs[0,3] + lhs[3,1] * rhs[1,3] + lhs[3,2] * rhs[2,3] + lhs[3,3] * rhs[3,3];

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

        public static NitroxMatrix4x4 operator *(float lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result;
            result.M = new float[4, 4];

            for (int i = 0; i < 4; i++)
            {
                result[i, i] = lhs * rhs[i, i];
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

        public NitroxVector3 MultiplyPoint(NitroxVector3 localPosition)
        {
            float x = M[0, 0] * localPosition.X + M[0, 1] * localPosition.Y + M[0, 2] * localPosition.Z + M[0, 3];
            float y = M[1, 0] * localPosition.X + M[1, 1] * localPosition.Y + M[1, 2] * localPosition.Z + M[1, 3];
            float z = M[2, 0] * localPosition.X + M[2, 1] * localPosition.Y + M[2, 2] * localPosition.Z + M[2, 3];
            float w = M[3, 0] * localPosition.X + M[3, 1] * localPosition.Y + M[3, 2] * localPosition.Z + M[3, 3];

            if (w == 1)
            {
                return new NitroxVector3(x, y, z);
            }
            else
            {
                NitroxVector3 vector = new NitroxVector3(x, y, z) / w;
                return vector;
            }
        }

        public static NitroxMatrix4x4 TRS(NitroxVector3 localPos, NitroxQuaternion localRotation, NitroxVector3 localScale)
        {
            NitroxMatrix4x4 scaleMatrix = SetScale(localScale);
            NitroxMatrix4x4 rotationMatrix = SetRotation(localRotation);
            NitroxMatrix4x4 translationMatrix = SetTranslation(localPos);
            NitroxMatrix4x4 result = translationMatrix * rotationMatrix * scaleMatrix;
            return result;
        }

        public static NitroxVector3 ExtractScale(ref NitroxMatrix4x4 matrix)
        {
            NitroxVector3 scale;
            scale.X = NitroxVector3.Length(new NitroxVector3(matrix[0, 0], matrix[0, 1], matrix[0, 2]));
            scale.Y = NitroxVector3.Length(new NitroxVector3(matrix[1, 0], matrix[1, 1], matrix[1, 2]));
            scale.Z = NitroxVector3.Length(new NitroxVector3(matrix[2, 0], matrix[2, 1], matrix[2, 2]));

            matrix[0, 0] /= scale.X;
            matrix[0, 1] /= scale.X;
            matrix[0, 2] /= scale.X;

            matrix[1, 0] /= scale.Y;
            matrix[1, 1] /= scale.Y;
            matrix[1, 2] /= scale.Y;

            matrix[2, 0] /= scale.Z;
            matrix[2, 1] /= scale.Z;
            matrix[2, 2] /= scale.Z;

            return scale;
        }

        public static NitroxQuaternion ExtractRotation(ref NitroxMatrix4x4 matrix)
        {
            NitroxQuaternion q = new NitroxQuaternion(0f, 0f, 0f, 1f);

            float trace = matrix[0,0] + matrix[1,1] + matrix[2,2];
            if (trace > 0)
            {
                float s = 0.5f / (float)Math.Sqrt(trace + 1.0f);
                q.W = 0.25f / s;
                q.X = (matrix[2, 1] - matrix[1, 2]) * s;
                q.Y = (matrix[0, 2] - matrix[2, 0]) * s;
                q.Z = (matrix[1, 0] - matrix[0, 1]) * s;
            }
            else
            {
                if (matrix[0, 0] > matrix[1, 1] && matrix[0, 0] > matrix[2, 2])
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + matrix[0, 0] - matrix[1, 1] - matrix[2, 2]);
                    q.W = (matrix[2, 1] - matrix[1, 2]) / s;
                    q.X = 0.25f * s;
                    q.Y = (matrix[0, 1] + matrix[1, 0]) / s;
                    q.Z = (matrix[0, 2] + matrix[2, 0]) / s;
                }
                else if (matrix[1, 1] > matrix[2, 2])
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + matrix[1, 1] - matrix[0, 0] - matrix[2, 2]);
                    q.W = (matrix[0, 2] - matrix[2, 0]) / s;
                    q.X = (matrix[0, 1] + matrix[1, 0]) / s;
                    q.Y = 0.25f * s;
                    q.Z = (matrix[1, 2] + matrix[2, 1]) / s;
                }
                else
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + matrix[2, 2] - matrix[0, 0] - matrix[1, 1]);
                    q.W = (matrix[1, 0] - matrix[0, 1]) / s;
                    q.W = (matrix[0, 2] + matrix[2, 0]) / s;
                    q.Y = (matrix[1, 2] + matrix[2, 1]) / s;
                    q.Z = 0.25f * s;
                }
            }

            NitroxQuaternion.Normalize(q);

            return q;
        }

        public static void DecomposeMatrix(ref NitroxMatrix4x4 matrix, out NitroxVector3 localPosition, out NitroxQuaternion localRotation, out NitroxVector3 localScale)
        {
            NitroxMatrix4x4 before = new NitroxMatrix4x4(matrix);

            localScale = ExtractScale(ref matrix);
            localRotation = ExtractRotation(ref matrix);
            localPosition = ExtractTranslation(ref matrix);

            matrix = before;
        }
    }
}
