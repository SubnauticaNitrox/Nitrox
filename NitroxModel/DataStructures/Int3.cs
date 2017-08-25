using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class Int3
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public static Int3 zero = new Int3(0, 0, 0);

        public Int3(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(Int3) != obj.GetType())
                return false;

            Int3 int3 = (Int3)obj;

            return (X == int3.X) &&
                   (Y == int3.Y) &&
                   (Z == int3.Z);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return "[Int3 - {" + X + ", " + Y + ", " + Z + "}]";
        }
    }
}
