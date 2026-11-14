using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Bases;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;

/// <summary>
/// A bulkhead door built into a base. Base geometry is rebuilt by the game, so the door GameObject has no identity of its own:
/// this entity gives it a server-owned id, matched to the door through the face it sits on.
/// The open state is stored in <see cref="Entity.Metadata"/> as a <see cref="BulkheadDoorMetadata"/>.
/// </summary>
[Serializable, DataContract]
public class BulkheadDoorEntity : GlobalRootEntity
{
    /// <summary>
    /// Face holding the door. The cell is relative to the base's anchor (anchor-independent, like <see cref="InteriorPieceEntity.BaseFace"/>)
    /// </summary>
    [DataMember(Order = 1)]
    public NitroxBaseFace Face { get; set; }

    [IgnoreConstructor]
    protected BulkheadDoorEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public BulkheadDoorEntity(NitroxId id, NitroxId parentId, NitroxBaseFace face, BulkheadDoorMetadata metadata)
    {
        Id = id;
        ParentId = parentId;
        Face = face;
        Metadata = metadata;
    }

    /// <remarks>
    /// Used for deserialization.
    /// <see cref="WorldEntity.SpawnedByServer"/> is set to true because this entity is meant to receive simulation locks
    /// </remarks>
    public BulkheadDoorEntity(NitroxBaseFace face, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, true, id, techType, metadata, parentId, childEntities)
    {
        Face = face;
    }

    public override string ToString()
    {
        return $"[BulkheadDoorEntity Id: {Id}, ParentId: {ParentId}, Face: {Face}, Metadata: {Metadata}]";
    }
}
