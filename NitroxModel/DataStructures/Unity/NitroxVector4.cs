using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.Unity;

[Serializable]
[JsonContractTransition]
public struct NitroxVector4 : IEquatable<NitroxVector4>
{
    [JsonMemberTransition]
    public float X;
    [JsonMemberTransition]
    public float Y;
    [JsonMemberTransition]
    public float Z;
    [JsonMemberTransition]
    public float W;

    public NitroxVector4(float x, float y, float z, float w)
    {
        W = w;
        X = x;
        Y = y;
        Z = z;
    }

    public float Magnitude => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

    public bool Equals(NitroxVector4 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
    }

    public override bool Equals(object obj)
    {
        return obj is NitroxVector4 other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = X.GetHashCode();
            hashCode = (hashCode * 397) ^ Y.GetHashCode();
            hashCode = (hashCode * 397) ^ Z.GetHashCode();
            hashCode = (hashCode * 397) ^ W.GetHashCode();
            return hashCode;
        }
    }
}
