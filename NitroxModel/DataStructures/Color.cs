using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    [Serializable]
    [ProtoContract]
    public class Color
    {
        private Color()
        {
            // Serialization ctor
        }

        public Color(float r, float g, float b) : this(r, g, b, 1)
        {
        }

        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color White { get; } = new Color(1, 1, 1, 1);

        [ProtoMember(1)]
        public float R { get; }

        [ProtoMember(2)]
        public float G { get; }

        [ProtoMember(3)]
        public float B { get; }

        [ProtoMember(4)]
        public float A { get; }

        public static implicit operator Color(Vector4 vector)
        {
            return new Color(vector.X, vector.Y, vector.Z, vector.W);
        }
    }
}
