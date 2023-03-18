using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class SubNameInputMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public string Name { get; }

    [DataMember(Order = 2)]
    public NitroxVector3[] Colors { get; }

    [IgnoreConstructor]
    protected SubNameInputMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public SubNameInputMetadata(string name, NitroxVector3[] colors)
    {
        Name = name;
        Colors = colors;
    }

    public override string ToString()
    {
        return $"[SubNameInputMetadata Name: {Name}, Colors: {Colors}]";
    }
}
