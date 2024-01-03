using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.Unity;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
[ProtoInclude(50, typeof(VehicleMetadata))]
[ProtoInclude(51, typeof(SubNameInputMetadata))]
public abstract class NamedColoredMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public string Name { get; init; }

    [DataMember(Order = 2)]
    public NitroxVector3[] Colors { get; init; }

    [IgnoreConstructor]
    protected NamedColoredMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public NamedColoredMetadata(string name, NitroxVector3[] colors) : base()
    {
        Name = name;
        Colors = colors;
    }

    public override string ToString()
    {
        return $"[{nameof(NamedColoredMetadata)} Name: {Name}, Colors: {string.Join(";", Colors)} {base.ToString()}]";
    }
}
