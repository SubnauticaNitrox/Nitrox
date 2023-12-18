using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class BeaconMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public string Label { get; }

    [IgnoreConstructor]
    protected BeaconMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public BeaconMetadata(string label)
    {
        Label = label;
    }

    public override string ToString()
    {
        return $"[BeaconMetadata Label: {Label}]";
    }
}
