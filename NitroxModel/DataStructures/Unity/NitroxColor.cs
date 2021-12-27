using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.Unity
{
    [ProtoContract]
    [ZeroFormattable]
    public struct NitroxColor
    {
        [ProtoMember(1)]
        [Index(0)]
        public float R { get; private set; }

        [ProtoMember(2)]
        [Index(1)]
        public float G { get; private set; }

        [ProtoMember(3)]
        [Index(2)]
        public float B { get; private set; }

        [ProtoMember(4)]
        [Index(3)]
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
