using System;
using NitroxModel.Helper;

namespace NitroxModel.DataStructures.GameLogic
{
    public struct NitroxMatrix4x4
    {
        public float M11, M12, M13, M14,
            M21, M22, M23, M24,
            M31, M32, M33, M34,
            M41, M42, M43, M44;

        public override string ToString()
        {
            return $"[\n[{M11}], [{M12}], [{M13}], [{M14}],\n" +
                $"[{M21}], [{M22}], [{M23}], [{M24}],\n" +
                $"[{M31}], [{M32}], [{M33}], [{M34}],\n" +
                $"[{M41}], [{M42}], [{M43}], [{M44}],\n]";
        }

        public static NitroxMatrix4x4 Identity { get; } = new NitroxMatrix4x4
            (
            1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f
            );

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

        public void SetColumn(int index, NitroxVector4 column)
        {
            switch (index)
            {
                case 0:
                    M11 = column.X;
                    M21 = column.Y;
                    M31 = column.Z;
                    M41 = column.W;
                    break;
                case 1:
                    M12 = column.X;
                    M22 = column.Y;
                    M32 = column.Z;
                    M42 = column.W;
                    break;
                case 2:
                    M13 = column.X;
                    M23 = column.Y;
                    M33 = column.Z;
                    M43 = column.W;
                    break;
                case 3:
                    M14 = column.X;
                    M24 = column.Y;
                    M34 = column.Z;
                    M44 = column.W;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public NitroxVector4 GetColumn(int index)
        {
            switch (index)
            {
                case 0:
                    return new NitroxVector4(M11, M21, M31, M41);
                case 1:
                    return new NitroxVector4(M12, M22, M32, M42);
                case 2:
                    return new NitroxVector4(M13, M23, M33, M43);
                case 3:
                    return new NitroxVector4(M14, M24, M34, M44);
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public void SetRow(int index, NitroxVector4 row)
        {
            switch (index)
            {
                case 0:
                    M11 = row.X;
                    M12 = row.Y;
                    M13 = row.Z;
                    M14 = row.W;
                    break;
                case 1:
                    M21 = row.X;
                    M22 = row.Y;
                    M23 = row.Z;
                    M24 = row.W;
                    break;
                case 2:
                    M31 = row.X;
                    M32 = row.Y;
                    M33 = row.Z;
                    M34 = row.W;
                    break;
                case 3:
                    M41 = row.X;
                    M42 = row.Y;
                    M43 = row.Z;
                    M44 = row.W;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public NitroxVector4 GetRow(int index)
        {
            switch (index)
            {
                case 0:
                    return new NitroxVector4(M11, M12, M13, M14);
                case 1:
                    return new NitroxVector4(M21, M22, M23, M24);
                case 2:
                    return new NitroxVector4(M31, M32, M33, M34);
                case 3:
                    return new NitroxVector4(M41, M42, M43, M44);
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public static NitroxMatrix4x4 SetScale(NitroxVector3 localScale)
        {
            NitroxMatrix4x4 scaleMatrix = NitroxMatrix4x4.Identity;
            scaleMatrix.M11 = 1f;
            scaleMatrix.M22 = 1f;
            scaleMatrix.M33 = 1f;
            scaleMatrix.M44 = 1f;


            scaleMatrix.M11 = localScale.X;
            scaleMatrix.M22 = localScale.Y;
            scaleMatrix.M33 = localScale.Z;

            return scaleMatrix;
        }

        public static NitroxVector3 GetScale(ref NitroxMatrix4x4 matrix)
        {
            NitroxVector3 scale;
            scale.X = NitroxVector3.Length(matrix.GetColumn(0));
            scale.Y = NitroxVector3.Length(matrix.GetColumn(1));
            scale.Z = NitroxVector3.Length(matrix.GetColumn(2));

            return scale;
        }

        public static NitroxMatrix4x4 GetRotationMatrix(NitroxQuaternion rotation)
        {
            NitroxVector3 radEuler = rotation.Euler * Mathf.DEG2RAD;
            float sinX = Mathf.Sin(radEuler.X);
            float cosX = Mathf.Cos(radEuler.X);
            float sinY = Mathf.Sin(radEuler.Y);
            float cosY = Mathf.Cos(radEuler.Y);
            float sinZ = Mathf.Sin(radEuler.Z);
            float cosZ = Mathf.Cos(radEuler.Z);

            NitroxMatrix4x4 matrix = new NitroxMatrix4x4();
            matrix.SetColumn(0, new NitroxVector4(
                cosY * cosZ,
                cosX * sinZ + sinX * sinY * cosZ,
                sinX * sinZ - cosX * sinY * cosZ,
                0f));

            matrix.SetColumn(1, new NitroxVector4(
                -cosY * sinZ,
                cosX * cosZ - sinX * sinY * sinZ,
                sinX * cosZ + cosX * sinY * sinZ,
                0f));

            matrix.SetColumn(2, new NitroxVector4(
                sinY,
                -sinX * cosY,
                cosX * cosY,
                0f));

            matrix.SetColumn(3, new NitroxVector4(0,0,0,1f));

            return matrix;
        }

        public static NitroxMatrix4x4 SetRotation(NitroxQuaternion localRotation)
        {
            NitroxQuaternion rot = NitroxQuaternion.Normalize(localRotation);
            NitroxMatrix4x4 rotationMatrix = GetRotationMatrix(localRotation);

            return rotationMatrix;
        }

        public static NitroxQuaternion GetRotation(ref NitroxMatrix4x4 matrix)
        {
            NitroxQuaternion q = NitroxQuaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));


            return q;
        }

        public static NitroxMatrix4x4 SetTranslation(NitroxVector3 localPosition)
        {
            NitroxMatrix4x4 transMatrix = NitroxMatrix4x4.Identity;



            transMatrix.M14 = localPosition.X;
            transMatrix.M24 = localPosition.Y;
            transMatrix.M34 = localPosition.Z;

            return transMatrix;
        }

        public static NitroxVector3 ExtractTranslation(ref NitroxMatrix4x4 matrix)
        {
            NitroxVector3 position;
            position.X = matrix.M14;
            position.Y = matrix.M24;
            position.Z = matrix.M34;
            return position;
        }

        public static NitroxMatrix4x4 TRS(NitroxVector3 localPos, NitroxQuaternion localRotation, NitroxVector3 localScale)
        {
            NitroxMatrix4x4 scaleMatrix = SetScale(localScale);
            NitroxMatrix4x4 rotationMatrix = SetRotation(localRotation);
            NitroxMatrix4x4 translationMatrix = SetTranslation(localPos);
            NitroxMatrix4x4 result = translationMatrix * rotationMatrix * scaleMatrix;
            return result;
        }

        public static void DecomposeMatrix(ref NitroxMatrix4x4 matrix, out NitroxVector3 localPosition, out NitroxQuaternion localRotation, out NitroxVector3 localScale)
        {
            NitroxMatrix4x4 before = new NitroxMatrix4x4(matrix);

            localScale = GetScale(ref matrix);
            NitroxMatrix4x4 matrixWithoutScale = SetScale(localScale).Inverse * matrix;
            localRotation = GetRotation(ref matrixWithoutScale);
            localPosition = matrix.GetColumn(3);

            matrix = before;
        }

        public static NitroxMatrix4x4 operator *(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result = NitroxMatrix4x4.Identity;

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
            NitroxMatrix4x4 result;

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
            NitroxMatrix4x4 result;

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

        public static NitroxMatrix4x4 operator -(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result;

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

        public static bool operator ==(NitroxMatrix4x4 left, NitroxMatrix4x4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NitroxMatrix4x4 left, NitroxMatrix4x4 right)
        {
            return !(left == right);
        }


        public override bool Equals(object obj)
        {
            return obj is NitroxMatrix4x4 x &&
                   M11 == x.M11 &&
                   M12 == x.M12 &&
                   M13 == x.M13 &&
                   M14 == x.M14 &&
                   M21 == x.M21 &&
                   M22 == x.M22 &&
                   M23 == x.M23 &&
                   M24 == x.M24 &&
                   M31 == x.M31 &&
                   M32 == x.M32 &&
                   M33 == x.M33 &&
                   M34 == x.M34 &&
                   M41 == x.M41 &&
                   M42 == x.M42 &&
                   M43 == x.M43 &&
                   M44 == x.M44;
        }

        public bool Equals(object obj, float tolerance)
        {
            return obj is NitroxMatrix4x4 x &&
                   (M11 == x.M11 || x.M11 >= M11 - tolerance && x.M11 <= M11 + tolerance) &&
                   (M12 == x.M12 || x.M12 >= M12 - tolerance && x.M12 <= M12 + tolerance) &&
                   (M13 == x.M13 || x.M13 >= M13 - tolerance && x.M13 <= M13 + tolerance) &&
                   (M14 == x.M14 || x.M14 >= M14 - tolerance && x.M14 <= M14 + tolerance) &&
                   (M21 == x.M21 || x.M21 >= M21 - tolerance && x.M21 <= M21 + tolerance) &&
                   (M22 == x.M22 || x.M22 >= M22 - tolerance && x.M22 <= M22 + tolerance) &&
                   (M23 == x.M23 || x.M23 >= M23 - tolerance && x.M23 <= M23 + tolerance) &&
                   (M24 == x.M24 || x.M24 >= M24 - tolerance && x.M24 <= M24 + tolerance) &&
                   (M31 == x.M31 || x.M31 >= M31 - tolerance && x.M31 <= M31 + tolerance) &&
                   (M32 == x.M32 || x.M32 >= M32 - tolerance && x.M32 <= M32 + tolerance) &&
                   (M33 == x.M33 || x.M33 >= M33 - tolerance && x.M33 <= M33 + tolerance) &&
                   (M34 == x.M34 || x.M34 >= M34 - tolerance && x.M34 <= M34 + tolerance) &&
                   (M41 == x.M41 || x.M41 >= M41 - tolerance && x.M41 <= M41 + tolerance) &&
                   (M42 == x.M42 || x.M42 >= M42 - tolerance && x.M42 <= M42 + tolerance) &&
                   (M43 == x.M43 || x.M43 >= M43 - tolerance && x.M43 <= M43 + tolerance) &&
                   (M44 == x.M44 || x.M44 >= M44 - tolerance && x.M44 <= M44 + tolerance);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -1955208504;
                hashCode = hashCode * -1521134295 + M11.GetHashCode();
                hashCode = hashCode * -1521134295 + M12.GetHashCode();
                hashCode = hashCode * -1521134295 + M13.GetHashCode();
                hashCode = hashCode * -1521134295 + M14.GetHashCode();
                hashCode = hashCode * -1521134295 + M21.GetHashCode();
                hashCode = hashCode * -1521134295 + M22.GetHashCode();
                hashCode = hashCode * -1521134295 + M23.GetHashCode();
                hashCode = hashCode * -1521134295 + M24.GetHashCode();
                hashCode = hashCode * -1521134295 + M31.GetHashCode();
                hashCode = hashCode * -1521134295 + M32.GetHashCode();
                hashCode = hashCode * -1521134295 + M33.GetHashCode();
                hashCode = hashCode * -1521134295 + M34.GetHashCode();
                hashCode = hashCode * -1521134295 + M41.GetHashCode();
                hashCode = hashCode * -1521134295 + M42.GetHashCode();
                hashCode = hashCode * -1521134295 + M43.GetHashCode();
                hashCode = hashCode * -1521134295 + M44.GetHashCode();
                return hashCode;
            }
        }
    }
}
