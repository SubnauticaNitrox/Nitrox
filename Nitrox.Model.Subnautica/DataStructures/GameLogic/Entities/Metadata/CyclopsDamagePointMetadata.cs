using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable] [DataContract]
public class CyclopsDamagePointMetadata : EntityMetadata
{
    [IgnoreConstructor]
    protected CyclopsDamagePointMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CyclopsDamagePointMetadata(float health)
    {
        Health = health;
    }

    [DataMember(Order = 1)]
    public float Health { get; set; }

    public override string ToString()
    {
        return $"[CyclopsDamagePointMetadata Health: {Health}]";
    }
}
