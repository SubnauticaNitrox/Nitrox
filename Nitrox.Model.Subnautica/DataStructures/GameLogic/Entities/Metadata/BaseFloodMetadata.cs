using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class BaseFloodMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float[] FlatValueGrid { get; }

    [IgnoreConstructor]
    protected BaseFloodMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public BaseFloodMetadata(float[] flatValueGrid)
    {
        FlatValueGrid = flatValueGrid;
    }

    public override string ToString()
    {
        return $"[BaseFloodMetadata FlatValueGrid.Length: {FlatValueGrid?.Length ?? 0}]";
    }
}
