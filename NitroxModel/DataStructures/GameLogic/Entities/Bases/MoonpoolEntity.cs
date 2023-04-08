using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class MoonpoolEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public NitroxInt3 Cell;

    [IgnoreConstructor]
    protected MoonpoolEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public MoonpoolEntity(NitroxId id, NitroxId parentId, NitroxInt3 cell)
    {
        Id = id;
        ParentId = parentId;
        Cell = cell;

        Transform = new();
    }

    /// <remarks>Used for deserialization</remarks>
    public MoonpoolEntity(NitroxInt3 cell, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        Cell = cell;

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
        return $"[MoonpoolEntity Id: {Id}, Cell: {Cell}, Children count: {ChildEntities.Count}]";
    }
}
