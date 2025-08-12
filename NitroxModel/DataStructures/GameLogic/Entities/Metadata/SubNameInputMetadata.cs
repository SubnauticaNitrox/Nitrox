using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class SubNameInputMetadata : NamedColoredMetadata
{
    [DataMember(Order = 1)]
    public int SelectedColorIndex { get; }

    [IgnoreConstructor]
    protected SubNameInputMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public SubNameInputMetadata(int selectedColorIndex, string name, NitroxVector3[] colors) : base(name, colors)
    {
        SelectedColorIndex = selectedColorIndex;
    }

    public override string ToString()
    {
        return $"[{nameof(SubNameInputMetadata)} SelectedColorIndex: {SelectedColorIndex} {base.ToString()}]";
    }
}
