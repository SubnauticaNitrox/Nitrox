using System.Numerics;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Helper
{
    public static class Matrix4x4Extension
    {
        public static Matrix4x4 Compose(NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 localScale)
        {
            Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(localPosition.X, localPosition.Y, localPosition.Z);
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromQuaternion((Quaternion)localRotation);
            Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(localScale.X, localScale.Y, localScale.Z);
            return scaleMatrix * rotationMatrix * translationMatrix;
        }

        public static Matrix4x4 Invert(this Matrix4x4 m)
        {
            Matrix4x4.Invert(m, out Matrix4x4 result);
            return result;
        }

        public static NitroxVector3 Transform(this Matrix4x4 m, NitroxVector3 v)
        {
            float x = v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + m.M41;
            float y = v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + m.M42;
            float z = v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + m.M43;
            float w = v.X * m.M14 + v.Y * m.M24 + v.Z * m.M34 + m.M44;
            w = 1f / w;

            return new NitroxVector3(x * w, y * w, z * w);
        }
    }
}
