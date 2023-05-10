using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class WaterParkCreatureEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public float Age;

    [DataMember(Order = 2)]
    public float TimeNextBreed;

    [DataMember(Order = 3)]
    public bool BornInside;

    [IgnoreConstructor]
    protected WaterParkCreatureEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public WaterParkCreatureEntity(float age, float timeNextBreed, bool bornInside, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        Age = age;
        TimeNextBreed = timeNextBreed;
        BornInside = bornInside;

        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        Transform = transform;
        ChildEntities = childEntities;
        Level = level;
        ClassId = classId;
        SpawnedByServer = spawnedByServer;
    }

    public override string ToString()
    {
        return $"[WaterParkCreatureEntity Id: {Id}, Age: {Age}, TimeNextBreed: {TimeNextBreed}, BornInside: {BornInside}]";
    }
}
