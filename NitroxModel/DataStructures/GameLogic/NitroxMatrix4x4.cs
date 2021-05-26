using System;
using NitroxModel.Helper;

namespace NitroxModel.DataStructures.GameLogic
{
    public struct NitroxMatrix4x4
    {
        public float M00, M01, M02, M03,
            M10, M11, M12, M13,
            M20, M21, M22, M23,
            M30, M31, M32, M33;

        public override string ToString()
        {
            return $"[\n[{M00}], [{M01}], [{M02}], [{M03}],\n" +
                $"[{M10}], [{M11}], [{M12}], [{M13}],\n" +
                $"[{M20}], [{M21}], [{M22}], [{M23}],\n" +
                $"[{M30}], [{M31}], [{M32}], [{M33}],\n]";
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

            float a = matrix.M00, b = matrix.M01, c = matrix.M02, d = matrix.M03;
            float e = matrix.M10, f = matrix.M11, g = matrix.M12, h = matrix.M13;
            float i = matrix.M20, j = matrix.M21, k = matrix.M22, l = matrix.M23;
            float m = matrix.M30, n = matrix.M31, o = matrix.M32, p = matrix.M33;

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

            result.M00 = a11 * invDet;
            result.M10 = a12 * invDet;
            result.M20 = a13 * invDet;
            result.M30 = a14 * invDet;

            result.M01 = -(b * kp_lo - c * jp_ln + d * jo_kn) * invDet;
            result.M11 = +(a * kp_lo - c * ip_lm + d * io_km) * invDet;
            result.M21 = -(a * jp_ln - b * ip_lm + d * in_jm) * invDet;
            result.M31 = +(a * jo_kn - b * io_km + c * in_jm) * invDet;

            float gp_ho = g * p - h * o;
            float fp_hn = f * p - h * n;
            float fo_gn = f * o - g * n;
            float ep_hm = e * p - h * m;
            float eo_gm = e * o - g * m;
            float en_fm = e * n - f * m;

            result.M02 = +(b * gp_ho - c * fp_hn + d * fo_gn) * invDet;
            result.M12 = -(a * gp_ho - c * ep_hm + d * eo_gm) * invDet;
            result.M22 = +(a * fp_hn - b * ep_hm + d * en_fm) * invDet;
            result.M32 = -(a * fo_gn - b * eo_gm + c * en_fm) * invDet;

            float gl_hk = g * l - h * k;
            float fl_hj = f * l - h * j;
            float fk_gj = f * k - g * j;
            float el_hi = e * l - h * i;
            float ek_gi = e * k - g * i;
            float ej_fi = e * j - f * i;

            result.M03 = -(b * gl_hk - c * fl_hj + d * fk_gj) * invDet;
            result.M13 = +(a * gl_hk - c * el_hi + d * ek_gi) * invDet;
            result.M23 = -(a * fl_hj - b * el_hi + d * ej_fi) * invDet;
            result.M33 = +(a * fk_gj - b * ek_gi + c * ej_fi) * invDet;

            return result;
        }

        public bool IsIdentity
        {
            get
            {
                return M00 == 1f && M11 == 1f && M22 == 1f && M33 == 1f &&
                    M01 == 0f && M02 == 0f && M03 == 0f &&
                    M10 == 0f && M12 == 0f && M13 == 0f &&
                    M20 == 0f && M21 == 0f && M23 == 0f &&
                    M30 == 0f && M31 == 0f && M32 == 0f;
            }
        }

        public NitroxMatrix4x4(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
        {
            M00 = m11;
            M01 = m12;
            M02 = m13;
            M03 = m14;
            M10 = m21;
            M11 = m22;
            M12 = m23;
            M13 = m24;
            M20 = m31;
            M21 = m32;
            M22 = m33;
            M23 = m34;
            M30 = m41;
            M31 = m42;
            M32 = m43;
            M33 = m44;
        }

        public NitroxMatrix4x4(NitroxMatrix4x4 matrix)
        {
            M00 = matrix.M00;
            M01 = matrix.M01;
            M02 = matrix.M02;
            M03 = matrix.M03;

            M10 = matrix.M10;
            M11 = matrix.M11;
            M12 = matrix.M12;
            M13 = matrix.M13;

            M20 = matrix.M20;
            M21 = matrix.M21;
            M22 = matrix.M22;
            M23 = matrix.M23;

            M30 = matrix.M30;
            M31 = matrix.M31;
            M32 = matrix.M32;
            M33 = matrix.M33;
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

            float a = M00, b = M01, c = M02, d = M03;
            float e = M10, f = M11, g = M12, h = M13;
            float i = M20, j = M21, k = M22, l = M23;
            float m = M30, n = M31, o = M32, p = M33;

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
            float x = M00 * localPosition.X + M01 * localPosition.Y + M02 * localPosition.Z + M03;
            float y = M10 * localPosition.X + M11 * localPosition.Y + M12 * localPosition.Z + M13;
            float z = M20 * localPosition.X + M21 * localPosition.Y + M22 * localPosition.Z + M23;
            float w = M30 * localPosition.X + M31 * localPosition.Y + M32 * localPosition.Z + M33;

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
                    M00 = column.X;
                    M10 = column.Y;
                    M20 = column.Z;
                    M30 = column.W;
                    break;
                case 1:
                    M01 = column.X;
                    M11 = column.Y;
                    M21 = column.Z;
                    M31 = column.W;
                    break;
                case 2:
                    M02 = column.X;
                    M12 = column.Y;
                    M22 = column.Z;
                    M32 = column.W;
                    break;
                case 3:
                    M03 = column.X;
                    M13 = column.Y;
                    M23 = column.Z;
                    M33 = column.W;
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
                    return new NitroxVector4(M00, M10, M20, M30);
                case 1:
                    return new NitroxVector4(M01, M11, M21, M31);
                case 2:
                    return new NitroxVector4(M02, M12, M22, M32);
                case 3:
                    return new NitroxVector4(M03, M13, M23, M33);
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public void SetRow(int index, NitroxVector4 row)
        {
            switch (index)
            {
                case 0:
                    M00 = row.X;
                    M01 = row.Y;
                    M02 = row.Z;
                    M03 = row.W;
                    break;
                case 1:
                    M10 = row.X;
                    M11 = row.Y;
                    M12 = row.Z;
                    M13 = row.W;
                    break;
                case 2:
                    M20 = row.X;
                    M21 = row.Y;
                    M22 = row.Z;
                    M23 = row.W;
                    break;
                case 3:
                    M30 = row.X;
                    M31 = row.Y;
                    M32 = row.Z;
                    M33 = row.W;
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
                    return new NitroxVector4(M00, M01, M02, M03);
                case 1:
                    return new NitroxVector4(M10, M11, M12, M13);
                case 2:
                    return new NitroxVector4(M20, M21, M22, M23);
                case 3:
                    return new NitroxVector4(M30, M31, M32, M33);
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public static NitroxMatrix4x4 GetScaleMatrix(NitroxVector3 localScale)
        {
            NitroxMatrix4x4 scaleMatrix = NitroxMatrix4x4.Identity;
            scaleMatrix.M00 = 1f;
            scaleMatrix.M11 = 1f;
            scaleMatrix.M22 = 1f;
            scaleMatrix.M33 = 1f;


            scaleMatrix.M00 = localScale.X;
            scaleMatrix.M11 = localScale.Y;
            scaleMatrix.M22 = localScale.Z;

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

        public static NitroxMatrix4x4 RotateX(float aAngleRad)
        {
            NitroxMatrix4x4 m = NitroxMatrix4x4.Identity;     //  1   0   0 
            m.M11 = m.M22 = Mathf.Cos(aAngleRad);             //  0  cos -sin
            m.M21 = Mathf.Sin(aAngleRad);                     //  0  sin  cos
            m.M12 = -m.M21;
            return m;
        }
        public static NitroxMatrix4x4 RotateY(float aAngleRad)
        {
            NitroxMatrix4x4 m = NitroxMatrix4x4.Identity;     // cos  0  sin
            m.M00 = m.M22 = Mathf.Cos(aAngleRad);             //  0   1   0 
            m.M02 = Mathf.Sin(aAngleRad);                     //-sin  0  cos
            m.M20 = -m.M02;
            return m;
        }
        public static NitroxMatrix4x4 RotateZ(float aAngleRad)
        {
            NitroxMatrix4x4 m = NitroxMatrix4x4.Identity;     // cos -sin 0
            m.M00 = m.M11 = Mathf.Cos(aAngleRad);             // sin  cos 0
            m.M10 = Mathf.Sin(aAngleRad);                     //  0   0   1
            m.M01 = -m.M10;
            return m;
        }
        public static NitroxMatrix4x4 Rotate(NitroxVector3 aEulerAngles)
        {
            NitroxVector3 rad = aEulerAngles * Mathf.DEG2RAD;
            NitroxMatrix4x4 rotX = RotateX(rad.Y);
            NitroxMatrix4x4 rotY = RotateY(rad.X);
            NitroxMatrix4x4 rotZ = RotateZ(rad.Z);

            return rotY * rotX * rotZ;

            /*
             * rotY * rotX =
             * 
             * cosY,    cosX,           sinY * cosX
             * 0,       cosX,           -sinX
             * -sinY,   cosY * sinX,    cosY * cosX
             * 
             * 
             * 
             * rotY * rotX * rotZ =
             * 
             * cosY * cosZ + cosX * sinZ,           cosY * -sinZ + cosX * cosZ,             sinY * cosX
             * cosX * sinZ,                         cosX * cosZ,                            -sinX         
             * -sinY * cosZ + cosY * sinX * sinZ,   -sinY * -sinZ + cosY * sinX * cosZ,     cosY * cosX
             * 
             * 
            */
        }

        public static NitroxMatrix4x4 GetRotationMatrix(NitroxQuaternion rotation)
        {
            return Rotate(rotation.Euler);
        }

        public static NitroxMatrix4x4 SetRotation(NitroxQuaternion localRotation)
        {
            NitroxMatrix4x4 rotationMatrix = GetRotationMatrix(localRotation);

            return rotationMatrix;
        }

        public static NitroxQuaternion GetRotation(ref NitroxMatrix4x4 matrix)
        {

            NitroxVector3 vect = NitroxVector3.Zero;

            if (matrix.M12 > 0.998) // Singularity at north pole
            {
                //TODO
            }
            else if (matrix.M12 > -0.998) // Singularity at south pole
            {
                //TODO
            }
            else
            {
                vect.Z = -Mathf.Atan2(-matrix.M10, matrix.M11);
                vect.X = -Mathf.Asin(matrix.M12);
                vect.Y = -Mathf.Atan2(-matrix.M02, matrix.M22);

            }

            vect *= Mathf.RAD2DEG; // convert radians back to degrees

            NitroxQuaternion rotation = NitroxQuaternion.FromEuler(vect);

            return rotation;
        }

        public static NitroxMatrix4x4 GetTranslationMatrix(NitroxVector3 localPosition)
        {
            NitroxMatrix4x4 transMatrix = Identity;



            transMatrix.M03 = localPosition.X;
            transMatrix.M13 = localPosition.Y;
            transMatrix.M23 = localPosition.Z;

            return transMatrix;
        }

        public static NitroxVector3 ExtractTranslation(ref NitroxMatrix4x4 matrix)
        {
            NitroxVector3 position = matrix.GetColumn(3);
            return position;
        }

        public static NitroxMatrix4x4 TRS(NitroxVector3 localPos, NitroxQuaternion localRotation, NitroxVector3 localScale)
        {
            NitroxMatrix4x4 scaleMatrix = GetScaleMatrix(localScale);
            NitroxMatrix4x4 rotationMatrix = SetRotation(localRotation);
            NitroxMatrix4x4 translationMatrix = GetTranslationMatrix(localPos);
            NitroxMatrix4x4 result = translationMatrix * rotationMatrix * scaleMatrix;
            return result;
        }

        public static void DecomposeMatrix(ref NitroxMatrix4x4 matrix, out NitroxVector3 localPosition, out NitroxQuaternion localRotation, out NitroxVector3 localScale)
        {
            localPosition = ExtractTranslation(ref matrix);
            localScale = GetScale(ref matrix);
            NitroxMatrix4x4 rMatrix = matrix * GetScaleMatrix(localScale).Inverse;
            localRotation = GetRotation(ref rMatrix);
        }

        public static NitroxMatrix4x4 Transpose(NitroxMatrix4x4 matrix)
        {
            return new NitroxMatrix4x4(matrix.M00, matrix.M10, matrix.M20, matrix.M30,
                matrix.M01, matrix.M11, matrix.M21, matrix.M31,
                matrix.M02, matrix.M12, matrix.M22, matrix.M32,
                matrix.M03, matrix.M13, matrix.M23, matrix.M33);
        }

        public static NitroxMatrix4x4 operator *(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result = Identity;
            result.M00 = lhs.M00 * rhs.M00 + lhs.M01 * rhs.M10 + lhs.M02 * rhs.M20 + lhs.M03 * rhs.M30;
            result.M01 = lhs.M00 * rhs.M01 + lhs.M01 * rhs.M11 + lhs.M02 * rhs.M21 + lhs.M03 * rhs.M31;
            result.M02 = lhs.M00 * rhs.M02 + lhs.M01 * rhs.M12 + lhs.M02 * rhs.M22 + lhs.M03 * rhs.M32;
            result.M03 = lhs.M00 * rhs.M03 + lhs.M01 * rhs.M13 + lhs.M02 * rhs.M23 + lhs.M03 * rhs.M33;
            result.M10 = lhs.M10 * rhs.M00 + lhs.M11 * rhs.M10 + lhs.M12 * rhs.M20 + lhs.M13 * rhs.M30;
            result.M11 = lhs.M10 * rhs.M01 + lhs.M11 * rhs.M11 + lhs.M12 * rhs.M21 + lhs.M13 * rhs.M31;
            result.M12 = lhs.M10 * rhs.M02 + lhs.M11 * rhs.M12 + lhs.M12 * rhs.M22 + lhs.M13 * rhs.M32;
            result.M13 = lhs.M10 * rhs.M03 + lhs.M11 * rhs.M13 + lhs.M12 * rhs.M23 + lhs.M13 * rhs.M33;
            result.M20 = lhs.M20 * rhs.M00 + lhs.M21 * rhs.M10 + lhs.M22 * rhs.M20 + lhs.M23 * rhs.M30;
            result.M21 = lhs.M20 * rhs.M01 + lhs.M21 * rhs.M11 + lhs.M22 * rhs.M21 + lhs.M23 * rhs.M31;
            result.M22 = lhs.M20 * rhs.M02 + lhs.M21 * rhs.M12 + lhs.M22 * rhs.M22 + lhs.M23 * rhs.M32;
            result.M23 = lhs.M20 * rhs.M03 + lhs.M21 * rhs.M13 + lhs.M22 * rhs.M23 + lhs.M23 * rhs.M33;
            result.M30 = lhs.M30 * rhs.M00 + lhs.M31 * rhs.M10 + lhs.M32 * rhs.M20 + lhs.M33 * rhs.M30;
            result.M31 = lhs.M30 * rhs.M01 + lhs.M31 * rhs.M11 + lhs.M32 * rhs.M21 + lhs.M33 * rhs.M31;
            result.M32 = lhs.M30 * rhs.M02 + lhs.M31 * rhs.M12 + lhs.M32 * rhs.M22 + lhs.M33 * rhs.M32;
            result.M33 = lhs.M30 * rhs.M03 + lhs.M31 * rhs.M13 + lhs.M32 * rhs.M23 + lhs.M33 * rhs.M33;
            return result;
        }

        public static NitroxMatrix4x4 operator +(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result;

            result.M00 = lhs.M00 + rhs.M00;
            result.M01 = lhs.M01 + rhs.M01;
            result.M02 = lhs.M02 + rhs.M02;
            result.M03 = lhs.M03 + rhs.M03;

            result.M10 = lhs.M10 + rhs.M10;
            result.M11 = lhs.M11 + rhs.M11;
            result.M12 = lhs.M12 + rhs.M12;
            result.M13 = lhs.M13 + rhs.M13;

            result.M20 = lhs.M20 + rhs.M20;
            result.M21 = lhs.M21 + rhs.M21;
            result.M22 = lhs.M22 + rhs.M22;
            result.M23 = lhs.M23 + rhs.M23;

            result.M30 = lhs.M30 + rhs.M30;
            result.M31 = lhs.M31 + rhs.M31;
            result.M32 = lhs.M32 + rhs.M32;
            result.M33 = lhs.M33 + rhs.M33;

            return result;
        }

        public static NitroxMatrix4x4 operator *(NitroxMatrix4x4 lhs, float rhs)
        {
            NitroxMatrix4x4 result;

            result.M00 = lhs.M00 * rhs;
            result.M01 = lhs.M01 * rhs;
            result.M02 = lhs.M02 * rhs;
            result.M03 = lhs.M03 * rhs;

            result.M10 = lhs.M10 * rhs;
            result.M11 = lhs.M11 * rhs;
            result.M12 = lhs.M12 * rhs;
            result.M13 = lhs.M13 * rhs;

            result.M20 = lhs.M20 * rhs;
            result.M21 = lhs.M21 * rhs;
            result.M22 = lhs.M22 * rhs;
            result.M23 = lhs.M23 * rhs;

            result.M30 = lhs.M30 * rhs;
            result.M31 = lhs.M31 * rhs;
            result.M32 = lhs.M32 * rhs;
            result.M33 = lhs.M33 * rhs;

            return result;
        }

        public static NitroxMatrix4x4 operator -(NitroxMatrix4x4 lhs, NitroxMatrix4x4 rhs)
        {
            NitroxMatrix4x4 result;

            result.M00 = lhs.M00 - rhs.M00;
            result.M01 = lhs.M01 - rhs.M01;
            result.M02 = lhs.M02 - rhs.M02;
            result.M03 = lhs.M03 - rhs.M03;

            result.M10 = lhs.M10 - rhs.M10;
            result.M11 = lhs.M11 - rhs.M11;
            result.M12 = lhs.M12 - rhs.M12;
            result.M13 = lhs.M13 - rhs.M13;

            result.M20 = lhs.M20 - rhs.M20;
            result.M21 = lhs.M21 - rhs.M21;
            result.M22 = lhs.M22 - rhs.M22;
            result.M23 = lhs.M23 - rhs.M23;

            result.M30 = lhs.M30 - rhs.M30;
            result.M31 = lhs.M31 - rhs.M31;
            result.M32 = lhs.M32 - rhs.M32;
            result.M33 = lhs.M33 - rhs.M33;

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
                   M00 == x.M00 &&
                   M01 == x.M01 &&
                   M02 == x.M02 &&
                   M03 == x.M03 &&
                   M10 == x.M10 &&
                   M11 == x.M11 &&
                   M12 == x.M12 &&
                   M13 == x.M13 &&
                   M20 == x.M20 &&
                   M21 == x.M21 &&
                   M22 == x.M22 &&
                   M23 == x.M23 &&
                   M30 == x.M30 &&
                   M31 == x.M31 &&
                   M32 == x.M32 &&
                   M33 == x.M33;
        }

        public bool Equals(object obj, float tolerance)
        {
            return obj is NitroxMatrix4x4 x &&
                   (M00 == x.M00 || x.M00 >= M00 - tolerance && x.M00 <= M00 + tolerance) &&
                   (M01 == x.M01 || x.M01 >= M01 - tolerance && x.M01 <= M01 + tolerance) &&
                   (M02 == x.M02 || x.M02 >= M02 - tolerance && x.M02 <= M02 + tolerance) &&
                   (M03 == x.M03 || x.M03 >= M03 - tolerance && x.M03 <= M03 + tolerance) &&
                   (M10 == x.M10 || x.M10 >= M10 - tolerance && x.M10 <= M10 + tolerance) &&
                   (M11 == x.M11 || x.M11 >= M11 - tolerance && x.M11 <= M11 + tolerance) &&
                   (M12 == x.M12 || x.M12 >= M12 - tolerance && x.M12 <= M12 + tolerance) &&
                   (M13 == x.M13 || x.M13 >= M13 - tolerance && x.M13 <= M13 + tolerance) &&
                   (M20 == x.M20 || x.M20 >= M20 - tolerance && x.M20 <= M20 + tolerance) &&
                   (M21 == x.M21 || x.M21 >= M21 - tolerance && x.M21 <= M21 + tolerance) &&
                   (M22 == x.M22 || x.M22 >= M22 - tolerance && x.M22 <= M22 + tolerance) &&
                   (M23 == x.M23 || x.M23 >= M23 - tolerance && x.M23 <= M23 + tolerance) &&
                   (M30 == x.M30 || x.M30 >= M30 - tolerance && x.M30 <= M30 + tolerance) &&
                   (M31 == x.M31 || x.M31 >= M31 - tolerance && x.M31 <= M31 + tolerance) &&
                   (M32 == x.M32 || x.M32 >= M32 - tolerance && x.M32 <= M32 + tolerance) &&
                   (M33 == x.M33 || x.M33 >= M33 - tolerance && x.M33 <= M33 + tolerance);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -1955208504;
                hashCode = hashCode * -1521134295 + M00.GetHashCode();
                hashCode = hashCode * -1521134295 + M01.GetHashCode();
                hashCode = hashCode * -1521134295 + M02.GetHashCode();
                hashCode = hashCode * -1521134295 + M03.GetHashCode();
                hashCode = hashCode * -1521134295 + M10.GetHashCode();
                hashCode = hashCode * -1521134295 + M11.GetHashCode();
                hashCode = hashCode * -1521134295 + M12.GetHashCode();
                hashCode = hashCode * -1521134295 + M13.GetHashCode();
                hashCode = hashCode * -1521134295 + M20.GetHashCode();
                hashCode = hashCode * -1521134295 + M21.GetHashCode();
                hashCode = hashCode * -1521134295 + M22.GetHashCode();
                hashCode = hashCode * -1521134295 + M23.GetHashCode();
                hashCode = hashCode * -1521134295 + M30.GetHashCode();
                hashCode = hashCode * -1521134295 + M31.GetHashCode();
                hashCode = hashCode * -1521134295 + M32.GetHashCode();
                hashCode = hashCode * -1521134295 + M33.GetHashCode();
                return hashCode;
            }
        }
    }
}
