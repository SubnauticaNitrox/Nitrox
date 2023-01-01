using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.Unity
{
    [DataContract]
    [Serializable]
    public struct NitroxColor
    {
        [DataMember(Order = 1)]
        public float R { get; private set; }

        [DataMember(Order = 2)]
        public float G { get; private set; }

        [DataMember(Order = 3)]
        public float B { get; private set; }

        [DataMember(Order = 4)]
        public float A { get; private set; }

        public NitroxColor(float r, float g, float b, float a = 1)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public override string ToString()
        {
            return $"[NitroxColor: {R}, {G}, {B}, {A}]";
        }
    }
}
