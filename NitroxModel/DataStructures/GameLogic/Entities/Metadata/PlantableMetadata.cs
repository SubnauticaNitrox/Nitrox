using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class PlantableMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float Progress { get; }

    [IgnoreConstructor]
    protected PlantableMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PlantableMetadata(float progress)
    {
        Progress = progress;
    }

    public override string ToString()
    {
        return $"[PlantableMetadata Time: {Progress}]";
    }
}
