using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class MapRoomEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public NitroxInt3 Cell { get; set; }

    /// <summary>
    /// World-space center used by the Scanner Room hologram. Null for rooms loaded from saves created before this field existed.
    /// </summary>
    [DataMember(Order = 2, EmitDefaultValue = false)]
    public NitroxVector3? ScanOrigin { get; set; }

    [IgnoreConstructor]
    protected MapRoomEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public MapRoomEntity(NitroxId id, NitroxId parentId, NitroxInt3 cell)
        : this(id, parentId, cell, null)
    {
    }

    public MapRoomEntity(NitroxId id, NitroxId parentId, NitroxInt3 cell, NitroxVector3? scanOrigin)
    {
        Id = id;
        ParentId = parentId;
        Cell = cell;
        ScanOrigin = scanOrigin;

        Transform = new();
    }

    /// <remarks>
    /// Used for deserialization.
    /// <see cref="WorldEntity.SpawnedByServer"/> is set to true because this entity is meant to receive simulation locks
    /// </remarks>
    public MapRoomEntity(NitroxInt3 cell, NitroxVector3? scanOrigin, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, true, id, techType, metadata, parentId, childEntities)
    {
        Cell = cell;
        ScanOrigin = scanOrigin;
    }

    public override string ToString()
    {
        return $"[MapRoomEntity Id: {Id}, Cell: {Cell}, ScanOrigin: {ScanOrigin}]";
    }
}
