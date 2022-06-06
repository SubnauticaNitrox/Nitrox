using System;
using System.Collections.Generic;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

/// <summary>
///     TechType is the enum used in Subnautica for defining all the spawnable objects in the world. This includes food, enemies and bases.
/// </summary>
/// <remarks>
///     Shim tech type model to bridge the gap between original subnautica and BZ.
/// </remarks>
[Serializable]
[JsonContractTransition]
public class NitroxTechType : IEquatable<NitroxTechType>
{
    [JsonMemberTransition]
    public string Name { get; set; }

    public NitroxTechType(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object obj)
    {
        return obj is NitroxTechType other && Equals(other);
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
