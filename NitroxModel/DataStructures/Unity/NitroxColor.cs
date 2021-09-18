using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.Unity
{
    [ProtoContract]
    [Serializable]
    public struct NitroxColor
    {
        [ProtoMember(1)]
        public float R { get; private set; }

        [ProtoMember(2)]
        public float G { get; private set; }

        [ProtoMember(3)]
        public float B { get; private set; }

        [ProtoMember(4)]
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
