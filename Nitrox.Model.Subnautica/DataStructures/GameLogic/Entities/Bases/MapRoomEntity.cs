using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

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

    /// <summary>
    /// The last server-authoritative resource selection. Saves created before shared Scanner Room selection omit this field.
    /// </summary>
    [DataMember(Order = 3, EmitDefaultValue = false)]
    public ScannerRoomScanState ScanState { get; set; } = ScannerRoomScanState.Empty;

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
        : this(id, parentId, cell, scanOrigin, ScannerRoomScanState.Empty)
    {
    }

    public MapRoomEntity(NitroxId id, NitroxId parentId, NitroxInt3 cell, NitroxVector3? scanOrigin, ScannerRoomScanState scanState)
    {
        Id = id;
        ParentId = parentId;
        Cell = cell;
        ScanOrigin = scanOrigin;
        ScanState = scanState ?? ScannerRoomScanState.Empty;

        Transform = new();
    }

    /// <remarks>
    /// Used for deserialization.
    /// <see cref="WorldEntity.SpawnedByServer"/> is set to true because this entity is meant to receive simulation locks
    /// </remarks>
    public MapRoomEntity(NitroxInt3 cell, NitroxVector3? scanOrigin, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        this(cell, scanOrigin, ScannerRoomScanState.Empty, transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
    }

    /// <remarks>
    /// Used for deserialization.
    /// <see cref="WorldEntity.SpawnedByServer"/> is set to true because this entity is meant to receive simulation locks
    /// </remarks>
    public MapRoomEntity(NitroxInt3 cell, NitroxVector3? scanOrigin, ScannerRoomScanState scanState, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, true, id, techType, metadata, parentId, childEntities)
    {
        Cell = cell;
        ScanOrigin = scanOrigin;
        ScanState = scanState ?? ScannerRoomScanState.Empty;
    }

    public override string ToString()
    {
        return $"[MapRoomEntity Id: {Id}, Cell: {Cell}, ScanOrigin: {ScanOrigin}, ScanState: {ScanState}]";
    }
}
