using System;
using NitroxModel.Helper;

namespace NitroxModel.DataStructures.GameLogic
{
    public struct NitroxMatrix4x4 : IEquatable<NitroxMatrix4x4>
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

        public NitroxMatrix4x4 Inverse => Invert(this);

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

        public bool IsIdentity =>
            M00 == 1f && M11 == 1f && M22 == 1f && M33 == 1f &&
            M01 == 0f && M02 == 0f && M03 == 0f &&
            M10 == 0f && M12 == 0f && M13 == 0f &&
            M20 == 0f && M21 == 0f && M23 == 0f &&
            M30 == 0f && M31 == 0f && M32 == 0f;

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

            return new NitroxVector3(x, y, z) / w;
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
            return index switch
            {
                0 => new NitroxVector4(M00, M10, M20, M30),
                1 => new NitroxVector4(M01, M11, M21, M31),
                2 => new NitroxVector4(M02, M12, M22, M32),
                3 => new NitroxVector4(M03, M13, M23, M33),
                _ => throw new IndexOutOfRangeException()
            };
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
            return index switch
            {
                0 => new NitroxVector4(M00, M01, M02, M03),
                1 => new NitroxVector4(M10, M11, M12, M13),
                2 => new NitroxVector4(M20, M21, M22, M23),
                3 => new NitroxVector4(M30, M31, M32, M33),
                _ => throw new IndexOutOfRangeException(),
            };
        }

        private static NitroxMatrix4x4 GetScaleMatrix(NitroxVector3 localScale)
        {
            NitroxMatrix4x4 scaleMatrix = Identity;

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

        // From https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToMatrix/index.htm
        public static NitroxMatrix4x4 GetRotationMatrix(NitroxQuaternion rotation)
        {
            NitroxQuaternion normedRot = rotation;

            if (!normedRot.Normalized) // Check for normalized just in case
            {
                normedRot = NitroxQuaternion.Normalize(normedRot);
            }

            NitroxMatrix4x4 rotMat = Identity;
            float sqx = normedRot.X * normedRot.X;
            float sqy = normedRot.Y * normedRot.Y;
            float sqz = normedRot.Z * normedRot.Z;
            float sqw = normedRot.W * normedRot.W;

            float xy = normedRot.X * normedRot.Y;
            float xz = normedRot.X * normedRot.Z;
            float xw = normedRot.X * normedRot.W;

            float yz = normedRot.Y * normedRot.Z;
            float yw = normedRot.Y * normedRot.W;

            float zw = normedRot.Z * normedRot.W;


            rotMat.M00 = sqx - sqy - sqz + sqw;
            rotMat.M11 = -sqx + sqy - sqz + sqw;
            rotMat.M22 = -sqx - sqy + sqz + sqw;

            rotMat.M10 = 2f * (xy + zw);
            rotMat.M01 = 2f * (xy - zw);

            rotMat.M20 = 2f * (xz - yw);
            rotMat.M02 = 2f * (xz + yw);

            rotMat.M21 = 2f * (yz + xw);
            rotMat.M12 = 2f * (yz - xw);

            return rotMat;
        }

        // From https://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
        // left in case weirdness ensues with the simpler method
        public static NitroxQuaternion GetRotation2(ref NitroxMatrix4x4 matrix)
        {

            NitroxQuaternion rotation = NitroxQuaternion.Identity;
            float tr = matrix.M00 + matrix.M11 + matrix.M22;
            if (tr > 0f)
            {
                double s = 0.5d / Math.Sqrt(tr + 1d);
                rotation.W = (float)(0.25d / s);
                rotation.X = (float)((matrix.M21 - matrix.M12) * s);
                rotation.Y = (float)((matrix.M02 - matrix.M20) * s);
                rotation.Z = (float)((matrix.M10 - matrix.M01) * s);
                return rotation;
            }
            else
            {
                if (matrix.M00 > matrix.M11 && matrix.M00 > matrix.M22)
                {
                    double s = 2d * Math.Sqrt(1d + matrix.M00 - matrix.M11 - matrix.M22);
                    rotation.W = (float)((matrix.M21 - matrix.M12) / s);
                    rotation.X = (float)(0.25d * s);
                    rotation.Y = (float)((matrix.M01 + matrix.M10) / s);
                    rotation.Z = (float)((matrix.M02 + matrix.M20) / s);
                }
                return rotation;
                if (matrix.M00 > matrix.M11 && matrix.M00 > matrix.M22)
                {
                    double s = 2d * Math.Sqrt(1d + matrix.M00 - matrix.M11 - matrix.M22);
                    rotation.W = (float)((matrix.M21 - matrix.M12) / s);
                    rotation.X = (float)(0.25d * s);
                    rotation.Y = (float)((matrix.M01 + matrix.M10) / s);
                    rotation.Z = (float)((matrix.M02 + matrix.M20) / s);

                    return rotation;
                }
                else if (matrix.M11 > matrix.M22)
                {
                    double s = 2d * Math.Sqrt(1d + matrix.M11 - matrix.M00 - matrix.M22);
                    rotation.W = (float)((matrix.M02 + matrix.M20) / s);
                    rotation.X = (float)((matrix.M01 + matrix.M10) / s);
                    rotation.Y = (float)(0.25d * s);
                    rotation.Z = (float)((matrix.M12 - matrix.M21) / s);
                    return rotation;
                }
                else
                {
                    double s = 2d * Math.Sqrt(1d + matrix.M22 - matrix.M00 - matrix.M11);
                    rotation.W = (float)((matrix.M10 - matrix.M01) / s);
                    rotation.X = (float)((matrix.M02 + matrix.M20) / s);
                    rotation.Y = (float)((matrix.M12 + matrix.M21) / s);
                    rotation.Z = (float)(0.25d * s);

                    return rotation;
                }
            }
        }

        public static NitroxMatrix4x4 ExtractScale(ref NitroxMatrix4x4 matrix, out NitroxVector3 scale)
        {
            scale = GetScale(ref matrix);

            // Normalize Scale from Matrix4x4
            float m00 = matrix.M00 / scale.X;
            float m01 = matrix.M01 / scale.Y;
            float m02 = matrix.M02 / scale.Z;
            float m10 = matrix.M10 / scale.X;
            float m11 = matrix.M11 / scale.Y;
            float m12 = matrix.M12 / scale.Z;
            float m20 = matrix.M20 / scale.X;
            float m21 = matrix.M21 / scale.Y;
            float m22 = matrix.M22 / scale.Z;

            return new NitroxMatrix4x4(m00, m01, m02, matrix.M03,
                                       m10, m11, m12, matrix.M13,
                                       m20, m21, m22, matrix.M23,
                                       matrix.M30, matrix.M31, matrix.M32, matrix.M33);
        }

        // From https://answers.unity.com/questions/402280/how-to-decompose-a-trs-matrix.html
        /// <remarks>
        /// Assumes a pure rotation matrix
        /// </remarks>
        public static NitroxQuaternion GetRotation(ref NitroxMatrix4x4 matrix)
        {
            NitroxQuaternion q;
            q.W = Mathf.Sqrt(Mathf.Max(0, 1 + matrix.M00 + matrix.M11 + matrix.M22)) / 2;
            q.X = Mathf.Sqrt(Mathf.Max(0, 1 + matrix.M00 - matrix.M11 - matrix.M22)) / 2;
            q.Y = Mathf.Sqrt(Mathf.Max(0, 1 - matrix.M00 + matrix.M11 - matrix.M22)) / 2;
            q.Z = Mathf.Sqrt(Mathf.Max(0, 1 - matrix.M00 - matrix.M11 + matrix.M22)) / 2;
            q.X *= Mathf.Sign(q.X * (matrix.M21 - matrix.M12));
            q.Y *= Mathf.Sign(q.Y * (matrix.M02 - matrix.M20));
            q.Z *= Mathf.Sign(q.Z * (matrix.M10 - matrix.M01));


            return NitroxQuaternion.Normalize(q);
        }

        private static NitroxMatrix4x4 GetTranslationMatrix(NitroxVector3 localPosition)
        {
            NitroxMatrix4x4 transMatrix = Identity;

            transMatrix.M03 = localPosition.X;
            transMatrix.M13 = localPosition.Y;
            transMatrix.M23 = localPosition.Z;

            return transMatrix;
        }

        public static NitroxVector3 GetTranslation(ref NitroxMatrix4x4 matrix)
        {
            NitroxVector3 position = matrix.GetColumn(3);
            return position;
        }

        public static NitroxMatrix4x4 TRS(NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 localScale)
        {
            NitroxMatrix4x4 scaleMatrix = GetScaleMatrix(localScale);
            NitroxMatrix4x4 rotationMatrix = GetRotationMatrix(localRotation);
            NitroxMatrix4x4 translationMatrix = GetTranslationMatrix(localPosition);
            return translationMatrix * rotationMatrix * scaleMatrix;
        }

        public static void DecomposeMatrix(ref NitroxMatrix4x4 matrix, out NitroxVector3 localPosition, out NitroxQuaternion localRotation, out NitroxVector3 localScale)
        {
            localPosition = GetTranslation(ref matrix);
            NitroxMatrix4x4 rMatrix = ExtractScale(ref matrix, out localScale);
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
            NitroxMatrix4x4 result;
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
            return obj is NitroxMatrix4x4 x && Equals(x);
        }

        public bool Equals(NitroxMatrix4x4 other)
        {
            return Equals(other, float.Epsilon);
        }

        public bool Equals(NitroxMatrix4x4 other, float tolerance)
        {
            return (M00 == other.M00 || Math.Abs(other.M00 - M00) < tolerance) &&
                   (M01 == other.M01 || Math.Abs(other.M01 - M01) < tolerance) &&
                   (M02 == other.M02 || Math.Abs(other.M02 - M02) < tolerance) &&
                   (M03 == other.M03 || Math.Abs(other.M03 - M03) < tolerance) &&
                   (M10 == other.M10 || Math.Abs(other.M10 - M10) < tolerance) &&
                   (M11 == other.M11 || Math.Abs(other.M11 - M11) < tolerance) &&
                   (M12 == other.M12 || Math.Abs(other.M12 - M12) < tolerance) &&
                   (M13 == other.M13 || Math.Abs(other.M13 - M13) < tolerance) &&
                   (M20 == other.M20 || Math.Abs(other.M20 - M20) < tolerance) &&
                   (M21 == other.M21 || Math.Abs(other.M21 - M21) < tolerance) &&
                   (M22 == other.M22 || Math.Abs(other.M22 - M22) < tolerance) &&
                   (M23 == other.M23 || Math.Abs(other.M23 - M23) < tolerance) &&
                   (M30 == other.M30 || Math.Abs(other.M30 - M30) < tolerance) &&
                   (M31 == other.M31 || Math.Abs(other.M31 - M31) < tolerance) &&
                   (M32 == other.M32 || Math.Abs(other.M32 - M32) < tolerance) &&
                   (M33 == other.M33 || Math.Abs(other.M32 - M32) < tolerance);
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
