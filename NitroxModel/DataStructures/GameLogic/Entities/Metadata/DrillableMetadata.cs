using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class DrillableMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float[] ChunkHealth { get; }

    [DataMember(Order = 2)]
    public float TimeLastDrilled { get; }

    [IgnoreConstructor]
    protected DrillableMetadata()
    {
        //Constructor for serialization. Has to be "protected" for json serialization.
    }

    public DrillableMetadata(float[] chunkHealth, float timeLastDrilled)
    {
        ChunkHealth = chunkHealth;
        TimeLastDrilled = timeLastDrilled;
    }

    public override string ToString()
    {
        return $"[DrillableMetadata ChunkHealth: {string.Join(",", ChunkHealth)} TimeLastDrilled: {TimeLastDrilled}]";
    }
}
