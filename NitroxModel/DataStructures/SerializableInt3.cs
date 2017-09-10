using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class SerializableInt3
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public SerializableInt3(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override string ToString()
        {
            return "[Int3 - {" + X + ", " + Y + ", " + Z + "}]";
        }

        public Int3 toInt3()
        {
            return new Int3(X, Y, Z);
        }

        public static SerializableInt3 from(Int3 int3)
        {
            return new SerializableInt3(int3.x, int3.y, int3.z);
        }
    }
}
