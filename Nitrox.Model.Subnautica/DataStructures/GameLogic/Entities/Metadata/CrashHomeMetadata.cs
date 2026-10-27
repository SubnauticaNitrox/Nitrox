using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
public class CrashHomeMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float SpawnTime { get; }

    [DataMember(Order = 2)]
    public Optional<NitroxId> SpawnedCrashId { get; }

    [IgnoreConstructor]
    protected CrashHomeMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CrashHomeMetadata(float spawnTime, Optional<NitroxId> spawnedCrashId)
    {
        SpawnTime = spawnTime;
        SpawnedCrashId = spawnedCrashId;
    }

    public override string ToString()
    {
        return $"[{nameof(CrashHomeMetadata)} SpawnTime: {SpawnTime}, SpawnedCrashId: {SpawnedCrashId}]";
    }
}
