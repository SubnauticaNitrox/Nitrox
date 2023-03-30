using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class FireMetadata : EntityMetadata
{
    /// <summary>
    /// It might be slightly unintuitive but subnautica subtracts the fire's douse amount from a livemixin health field.
    /// Here we keep that health in sync as the fire is doused.
    /// </summary>
    [DataMember(Order = 1)]
    public float Health { get; }

    [IgnoreConstructor]
    protected FireMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public FireMetadata(float health)
    {
        Health = health;
    }

    public override string ToString()
    {
        return $"[FireMetadata Health: {Health}]";
    }
}
