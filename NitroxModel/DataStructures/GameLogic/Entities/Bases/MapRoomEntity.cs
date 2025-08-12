using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class MapRoomEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public NitroxInt3 Cell { get; set; }

    [IgnoreConstructor]
    protected MapRoomEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public MapRoomEntity(NitroxId id, NitroxId parentId, NitroxInt3 cell)
    {
        Id = id;
        ParentId = parentId;
        Cell = cell;

        Transform = new();
    }

    /// <remarks>
    /// Used for deserialization.
    /// <see cref="WorldEntity.SpawnedByServer"/> is set to true because this entity is meant to receive simulation locks
    /// </remarks>
    public MapRoomEntity(NitroxInt3 cell, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, true, id, techType, metadata, parentId, childEntities)
    {
        Cell = cell;
    }

    public override string ToString()
    {
        return $"[MapRoomEntity Id: {Id}, Cell: {Cell}]";
    }
}
