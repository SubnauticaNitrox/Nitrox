using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
public class CrashHomeMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float SpawnTime { get; }

    [IgnoreConstructor]
    protected CrashHomeMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CrashHomeMetadata(float spawnTime)
    {
        SpawnTime = spawnTime;
    }

    public override string ToString()
    {
        return $"[CrashHomeMetadata SpawnTime: {SpawnTime}]";
    }
}
