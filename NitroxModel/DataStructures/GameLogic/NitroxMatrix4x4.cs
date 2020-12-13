using System;

namespace NitroxModel.DataStructures.GameLogic
{
    public struct NitroxMatrix4x4
    {
        public float M11;
        public float M12;
        public float M13;
        public float M14;

        public float M21;
        public float M22;
        public float M23;
        public float M24;
        
        public float M31;
        public float M32;
        public float M33;
        public float M34;

        public float M41;
        public float M42;
        public float M43;
        public float M44;

        private static readonly NitroxMatrix4x4 identity = new NitroxMatrix4x4
            (
            1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f
            );

        public override string ToString()
        {
            return $"[\n[{M11}], [{M12}], [{M13}], [{M14}],\n" +
                $"[{M21}], [{M22}], [{M23}], [{M24}],\n" +
                $"[{M31}], [{M32}], [{M33}], [{M34}],\n" +
                $"[{M41}], [{M42}], [{M43}], [{M44}],\n]";
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
            NitroxMatrix4x4 result = Identity;

            float a = matrix.M11, b = matrix.M12, c = matrix.M13, d = matrix.M14;
            float e = matrix.M21, f = matrix.M22, g = matrix.M23, h = matrix.M24;
            float i = matrix.M31, j = matrix.M32, k = matrix.M33, l = matrix.M34;
            float m = matrix.M41, n = matrix.M42, o = matrix.M43, p = matrix.M44;

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

            result.M11 = a11 * invDet;
            result.M21 = a12 * invDet;
            result.M31 = a13 * invDet;
            result.M41 = a14 * invDet;

            result.M12 = -(b * kp_lo - c * jp_ln + d * jo_kn) * invDet;
            result.M22 = +(a * kp_lo - c * ip_lm + d * io_km) * invDet;
            result.M32 = -(a * jp_ln - b * ip_lm + d * in_jm) * invDet;
            result.M42 = +(a * jo_kn - b * io_km + c * in_jm) * invDet;

            float gp_ho = g * p - h * o;
            float fp_hn = f * p - h * n;
            float fo_gn = f * o - g * n;
            float ep_hm = e * p - h * m;
            float eo_gm = e * o - g * m;
            float en_fm = e * n - f * m;

            result.M13 = +(b * gp_ho - c * fp_hn + d * fo_gn) * invDet;
            result.M23 = -(a * gp_ho - c * ep_hm + d * eo_gm) * invDet;
            result.M33 = +(a * fp_hn - b * ep_hm + d * en_fm) * invDet;
            result.M43 = -(a * fo_gn - b * eo_gm + c * en_fm) * invDet;

            float gl_hk = g * l - h * k;
            float fl_hj = f * l - h * j;
            float fk_gj = f * k - g * j;
            float el_hi = e * l - h * i;
            float ek_gi = e * k - g * i;
            float ej_fi = e * j - f * i;

            result.M14 = -(b * gl_hk - c * fl_hj + d * fk_gj) * invDet;
            result.M24 = +(a * gl_hk - c * el_hi + d * ek_gi) * invDet;
            result.M34 = -(a * fl_hj - b * el_hi + d * ej_fi) * invDet;
            result.M44 = +(a * fk_gj - b * ek_gi + c * ej_fi) * invDet;

            return result;
        }

        public bool IsIdentity
        {
            get
            {
                return M11 == 1f && M22 == 1f && M33 == 1f && M44 == 1f &&
                    M12 == 0f && M13 == 0f && M14 == 0f &&
                    M21 == 0f && M23 == 0f && M24 == 0f &&
                    M31 == 0f && M32 == 0f && M34 == 0f &&
                    M41 == 0f && M42 == 0f && M43 == 0f;
            }
        }

        public NitroxMatrix4x4(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;
            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;
            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
        }

        public NitroxMatrix4x4(NitroxMatrix4x4 matrix)
        {
            M11 = matrix.M11;
            M12 = matrix.M12;
            M13 = matrix.M13;
            M14 = matrix.M14;
            M21 = matrix.M21;
            M22 = matrix.M22;
            M23 = matrix.M23;
            M24 = matrix.M24;
            M31 = matrix.M31;
            M32 = matrix.M32;
            M33 = matrix.M33;
            M34 = matrix.M34;
            M41 = matrix.M41;
            M42 = matrix.M42;
            M43 = matrix.M43;
            M44 = matrix.M44;
        }

        public static NitroxMatrix4x4 SetScale(NitroxVector3 localScale)
        {
            NitroxMatrix4x4 scaleMatrix = Identity;

            scaleMatrix.M11 = localScale.X;
            scaleMatrix.M22 = localScale.Y;
            scaleMatrix.M33 = localScale.Z;

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

            float a = M11, b = M12, c = M13, d = M14;
            float e = M21, f = M22, g = M23, h = M24;
            float i = M31, j = M32, k = M33, l = M34;
            float m = M41, n = M42, o = M43, p = M44;

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
            NitroxMatrix4x4 rotationMatrix = Identity;

            float sqw = rot.W * rot.W;
            float sqx = rot.X * rot.X;
            float sqy = rot.Y * rot.Y;
            float sqz = rot.Z * rot.Z;

            float invs = 1 / (sqx + sqy + sqz + sqw);
            rotationMatrix.M11 = (sqx - sqy - sqz + sqw) * invs;
            rotationMatrix.M22 = (-sqx + sqy - sqz + sqw) * invs;
            rotationMatrix.M33 = (-sqx - sqy + sqz + sqw) * invs;

            float tmp1 = rot.X * rot.Y;
            float tmp2 = rot.Z * rot.W;

            rotationMatrix.M21 = 2 * (tmp1 + tmp2) * invs;
            rotationMatrix.M12 = 2 * (tmp1 - tmp2) * invs;

            tmp1 = rot.X * rot.Z;
            tmp2 = rot.Y * rot.W;

            rotationMatrix.M31 = 2 * (tmp1 - tmp2) * invs;
            rotationMatrix.M13 = 2 * (tmp1 + tmp2) * invs;

            tmp1 = rot.Y * rot.Z;
            tmp2 = rot.X * rot.W;

            rotationMatrix.M32 = 2 * (tmp1 + tmp2) * invs;
            rotationMatrix.M23 = 2 * (tmp1 - tmp2) * invs;

            return rotationMatrix;
        }

        public static NitroxMatrix4x4 SetTranslation(NitroxVector3 localPosition)
        {
            NitroxMatrix4x4 transMatrix = Identity;



            transMatrix.M14 = localPosition.X;
            transMatrix.M24 = localPosition.Y;
            transMatrix.M34 = localPosition.Z;

            return transMatrix;
        }

        public static NitroxVector3 ExtractTranslation(ref NitroxMatrix4x4 matrix)
        {
            NitroxVector3 position = NitroxVector3.Zero;
            position.X = matrix.M14;
            position.Y = matrix.M24;
            position.Z = matrix.M34;
            return position;
        }

        public static NitroxMatrix4x4 operator *(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result = Identity;

            // First row
            result.M11 = lhs.M11 * rhs.M11 + lhs.M12 * rhs.M21 + lhs.M13 * rhs.M31 + lhs.M14 * rhs.M41;
            result.M12 = lhs.M11 * rhs.M12 + lhs.M12 * rhs.M22 + lhs.M13 * rhs.M32 + lhs.M14 * rhs.M42;
            result.M13 = lhs.M11 * rhs.M13 + lhs.M12 * rhs.M23 + lhs.M13 * rhs.M33 + lhs.M14 * rhs.M43;
            result.M14 = lhs.M11 * rhs.M14 + lhs.M12 * rhs.M24 + lhs.M13 * rhs.M34 + lhs.M14 * rhs.M44;

            // Second row
            result.M21 = lhs.M21 * rhs.M11 + lhs.M22 * rhs.M21 + lhs.M23 * rhs.M31 + lhs.M24 * rhs.M41;
            result.M22 = lhs.M21 * rhs.M12 + lhs.M22 * rhs.M22 + lhs.M23 * rhs.M32 + lhs.M24 * rhs.M42;
            result.M23 = lhs.M21 * rhs.M13 + lhs.M22 * rhs.M23 + lhs.M23 * rhs.M33 + lhs.M24 * rhs.M43;
            result.M24 = lhs.M21 * rhs.M14 + lhs.M22 * rhs.M24 + lhs.M23 * rhs.M34 + lhs.M24 * rhs.M44;

            // Third row
            result.M31 = lhs.M31 * rhs.M11 + lhs.M32 * rhs.M21 + lhs.M33 * rhs.M31 + lhs.M34 * rhs.M41;
            result.M32 = lhs.M31 * rhs.M12 + lhs.M32 * rhs.M22 + lhs.M33 * rhs.M32 + lhs.M34 * rhs.M42;
            result.M33 = lhs.M31 * rhs.M13 + lhs.M32 * rhs.M23 + lhs.M33 * rhs.M33 + lhs.M34 * rhs.M43;
            result.M34 = lhs.M31 * rhs.M14 + lhs.M32 * rhs.M24 + lhs.M33 * rhs.M34 + lhs.M34 * rhs.M44;

            // Fourth row
            result.M41 = lhs.M41 * rhs.M11 + lhs.M42 * rhs.M21 + lhs.M43 * rhs.M31 + lhs.M44 * rhs.M41;
            result.M42 = lhs.M41 * rhs.M12 + lhs.M42 * rhs.M22 + lhs.M43 * rhs.M32 + lhs.M44 * rhs.M42;
            result.M43 = lhs.M41 * rhs.M13 + lhs.M42 * rhs.M23 + lhs.M43 * rhs.M33 + lhs.M44 * rhs.M43;
            result.M44 = lhs.M41 * rhs.M14 + lhs.M42 * rhs.M24 + lhs.M43 * rhs.M34 + lhs.M44 * rhs.M44;

            return result;
        }

        public static NitroxMatrix4x4 operator +(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result = Identity;

            result.M11 = lhs.M11 + rhs.M11;
            result.M12 = lhs.M12 + rhs.M12;
            result.M13 = lhs.M13 + rhs.M13;
            result.M14 = lhs.M14 + rhs.M14;

            result.M21 = lhs.M21 + rhs.M21;
            result.M22 = lhs.M22 + rhs.M22;
            result.M23 = lhs.M23 + rhs.M23;
            result.M24 = lhs.M24 + rhs.M24;

            result.M31 = lhs.M31 + rhs.M31;
            result.M32 = lhs.M32 + rhs.M32;
            result.M33 = lhs.M33 + rhs.M33;
            result.M34 = lhs.M34 + rhs.M34;

            result.M41 = lhs.M41 + rhs.M41;
            result.M42 = lhs.M42 + rhs.M42;
            result.M43 = lhs.M43 + rhs.M43;
            result.M44 = lhs.M44 + rhs.M44;

            return result;
        }

        public static NitroxMatrix4x4 operator *(float lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result = Identity;

            result.M11 = lhs * rhs.M11;
            result.M12 = lhs * rhs.M12;
            result.M13 = lhs * rhs.M13;
            result.M14 = lhs * rhs.M14;

            result.M21 = lhs * rhs.M21;
            result.M22 = lhs * rhs.M22;
            result.M23 = lhs * rhs.M23;
            result.M24 = lhs * rhs.M24;

            result.M31 = lhs * rhs.M31;
            result.M32 = lhs * rhs.M32;
            result.M33 = lhs * rhs.M33;
            result.M34 = lhs * rhs.M34;

            result.M41 = lhs * rhs.M41;
            result.M42 = lhs * rhs.M42;
            result.M43 = lhs * rhs.M43;
            result.M44 = lhs * rhs.M44;


            return result;
        }

        public static bool operator ==(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            return lhs.IsIdentity && rhs.IsIdentity ||
                lhs.M11 == rhs.M11 && lhs.M12 == rhs.M12 && lhs.M13 == rhs.M13 && lhs.M14 == rhs.M14 &&
                lhs.M21 == rhs.M21 && lhs.M22 == rhs.M22 && lhs.M23 == rhs.M23 && lhs.M24 == rhs.M24 &&
                lhs.M31 == rhs.M31 && lhs.M32 == rhs.M32 && lhs.M33 == rhs.M33 && lhs.M34 == rhs.M34 &&
                lhs.M41 == rhs.M41 && lhs.M42 == rhs.M42 && lhs.M43 == rhs.M43 && lhs.M44 == rhs.M44;
        }

        public static bool operator !=(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            return !(lhs == rhs);
        }

        public static NitroxMatrix4x4 operator -(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result = Identity;

            result.M11 = lhs.M11 - rhs.M11;
            result.M12 = lhs.M12 - rhs.M12;
            result.M13 = lhs.M13 - rhs.M13;
            result.M14 = lhs.M14 - rhs.M14;

            result.M21 = lhs.M21 - rhs.M21;
            result.M22 = lhs.M22 - rhs.M22;
            result.M23 = lhs.M23 - rhs.M23;
            result.M24 = lhs.M24 - rhs.M24;

            result.M31 = lhs.M31 - rhs.M31;
            result.M32 = lhs.M32 - rhs.M32;
            result.M33 = lhs.M33 - rhs.M33;
            result.M34 = lhs.M34 - rhs.M34;

            result.M41 = lhs.M41 - rhs.M41;
            result.M42 = lhs.M42 - rhs.M42;
            result.M43 = lhs.M43 - rhs.M43;
            result.M44 = lhs.M44 - rhs.M44;

            return result;
        }

        public NitroxVector3 MultiplyPoint(NitroxVector3 localPosition)
        {
            float x = M11 * localPosition.X + M12 * localPosition.Y + M13 * localPosition.Z + M14;
            float y = M21 * localPosition.X + M22 * localPosition.Y + M23 * localPosition.Z + M24;
            float z = M31 * localPosition.X + M32 * localPosition.Y + M33 * localPosition.Z + M34;
            float w = M41 * localPosition.X + M42 * localPosition.Y + M43 * localPosition.Z + M44;

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
            NitroxVector3 scale = NitroxVector3.One;
            scale.X = NitroxVector3.Length(new NitroxVector3(matrix.M11, matrix.M12, matrix.M13));
            scale.Y = NitroxVector3.Length(new NitroxVector3(matrix.M21, matrix.M22, matrix.M23));
            scale.Z = NitroxVector3.Length(new NitroxVector3(matrix.M31, matrix.M32, matrix.M33));

            matrix.M11 /= scale.X;
            matrix.M12 /= scale.X;
            matrix.M13 /= scale.X;

            matrix.M21 /= scale.Y;
            matrix.M22 /= scale.Y;
            matrix.M23 /= scale.Y;

            matrix.M31 /= scale.Z;
            matrix.M32 /= scale.Z;
            matrix.M33 /= scale.Z;

            return scale;
        }

        public static NitroxQuaternion ExtractRotation(ref NitroxMatrix4x4 matrix)
        {
            NitroxQuaternion q = new NitroxQuaternion(0f, 0f, 0f, 1f);

            float trace = matrix.M11 + matrix.M22 + matrix.M33;
            if (trace > 0)
            {
                float s = 0.5f / (float)Math.Sqrt(trace + 1.0f);
                q.W = 0.25f / s;
                q.X = (matrix.M32 - matrix.M23) * s;
                q.Y = (matrix.M13 - matrix.M31) * s;
                q.Z = (matrix.M21 - matrix.M12) * s;
            }
            else
            {
                if (matrix.M11 > matrix.M22 && matrix.M11 > matrix.M33)
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + matrix.M11 - matrix.M22 - matrix.M33);
                    q.W = (matrix.M32 - matrix.M23) / s;
                    q.X = 0.25f * s;
                    q.Y = (matrix.M12 + matrix.M21) / s;
                    q.Z = (matrix.M13 + matrix.M31) / s;
                }
                else if (matrix.M22 > matrix.M33)
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + matrix.M22 - matrix.M11 - matrix.M33);
                    q.W = (matrix.M13 - matrix.M31) / s;
                    q.X = (matrix.M12 + matrix.M21) / s;
                    q.Y = 0.25f * s;
                    q.Z = (matrix.M23 + matrix.M32) / s;
                }
                else
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + matrix.M33 - matrix.M11 - matrix.M22);
                    q.W = (matrix.M21 - matrix.M12) / s;
                    q.W = (matrix.M13 + matrix.M31) / s;
                    q.Y = (matrix.M23 + matrix.M32) / s;
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
