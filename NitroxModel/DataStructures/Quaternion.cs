using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class Quaternion
    {
        public float W { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Quaternion(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public override string ToString()
        {
            return "[Quaternion - {" + X + ", " + Y + ", " + Z + "," + W + "}]";
        }
    }
}
