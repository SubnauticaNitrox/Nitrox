using System;

namespace NitroxModel.DataStructures.GameLogic
{
    public struct NitroxVector4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public NitroxVector4(float x, float y, float z, float w)
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
        }

        public float Magnitude
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
            }
        }
    }
}
