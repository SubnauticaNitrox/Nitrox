using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class Int3
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public Int3(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override string ToString()
        {
            return "[Int3 - {" + X + ", " + Y + ", " + Z + "}]";
        }
    }
}
