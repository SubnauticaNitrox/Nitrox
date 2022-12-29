using System;
using System.Collections.Generic;
using BinaryPack.Attributes;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic;

/// <summary>
///     TechType is the enum used in Subnautica for defining all the spawnable objects in the world. This includes food, enemies and bases.
/// </summary>
/// <remarks>
///     Shim tech type model to bridge the gap between original subnautica and BZ.
/// </remarks>
[ProtoContract]
[Serializable]
public class NitroxTechType : IEquatable<NitroxTechType>
{
    [ProtoMember(1)]
    public string Name { get; }

    [IgnoreConstructor]
    protected NitroxTechType()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public NitroxTechType(string name)
    {
        Name = name;
    }

    public static NitroxTechType None { get; } = new NitroxTechType("None");

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((NitroxTechType)obj);
    }

    public bool Equals(NitroxTechType other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
    }
}
