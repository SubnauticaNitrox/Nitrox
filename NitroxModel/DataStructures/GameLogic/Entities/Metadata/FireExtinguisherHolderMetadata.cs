using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class FireExtinguisherHolderMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool HasExtinguisher { get; }

    [DataMember(Order = 2)]
    public float Fuel { get; }

    [IgnoreConstructor]
    protected FireExtinguisherHolderMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public FireExtinguisherHolderMetadata(bool hasExtinguisher, float fuel)
    {
        HasExtinguisher = hasExtinguisher;
        Fuel = fuel;
    }

    public override string ToString()
    {
        return $"[FireExtinguisherHolderMetadata HasExtinguisher: {HasExtinguisher}, Fuel: {Fuel}]";
    }
}
