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

    [DataMember(Order = 2)]
    public List<bool> CameraDockingStates { get; set; } = [];

    [DataMember(Order = 3)]
    public List<NitroxId> CameraDockingIds { get; set; } = [];

    [IgnoreConstructor]
    protected MapRoomEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public MapRoomEntity(NitroxId id, NitroxId parentId, NitroxInt3 cell, List<bool>? cameraDockingStates = null, List<NitroxId>? cameraDockingIds = null)
    {
        Id = id;
        ParentId = parentId;
        Cell = cell;
        CameraDockingStates = cameraDockingStates ?? [];
        CameraDockingIds = cameraDockingIds ?? [];

        Transform = new();
    }

    /// <remarks>
    /// Used for deserialization.
    /// <see cref="WorldEntity.SpawnedByServer"/> is set to true because this entity is meant to receive simulation locks
    /// </remarks>
    public MapRoomEntity(NitroxInt3 cell, List<bool>? cameraDockingStates, List<NitroxId>? cameraDockingIds, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, true, id, techType, metadata, parentId, childEntities)
    {
        Cell = cell;
        CameraDockingStates = cameraDockingStates ?? [];
        CameraDockingIds = cameraDockingIds ?? [];
    }

    public override string ToString()
    {
        return $"[MapRoomEntity Id: {Id}, Cell: {Cell}]";
    }
}
