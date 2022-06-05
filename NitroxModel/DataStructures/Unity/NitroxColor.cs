using System;
using NitroxModel.Serialization;
using ProtoBufNet;

namespace NitroxModel.DataStructures.Unity;

[Serializable]
[ProtoContract, JsonContractTransition]
public struct NitroxColor
{
    [ProtoMember(1), JsonMemberTransition]
    public float R { get; private set; }

    [ProtoMember(2), JsonMemberTransition]
    public float G { get; private set; }

    [ProtoMember(3), JsonMemberTransition]
    public float B { get; private set; }

    [ProtoMember(4), JsonMemberTransition]
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
