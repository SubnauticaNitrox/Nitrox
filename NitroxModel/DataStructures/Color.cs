using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class Color
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public Color(float r, float g, float b, float a = 1f)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public override string ToString()
        {
            return "[Color - {" + R + ", " + G + ", " + B + ", " + A + "}]";
        }
    }
}
