using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
public abstract class VehicleMetadata : NamedColoredMetadata
{
    [DataMember(Order = 1)]
    public float Health { get; init; }

    [IgnoreConstructor]
    protected VehicleMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public VehicleMetadata(float health, string name, NitroxVector3[] colors) : base(name, colors)
    {
        Health = health;
    }

    public new string FieldsToString()
    {
        return $"Health: {Health}, {base.FieldsToString()}";
    }

    public override string ToString()
    {
        return $"[{nameof(VehicleMetadata)} {FieldsToString()}]";
    }
}
