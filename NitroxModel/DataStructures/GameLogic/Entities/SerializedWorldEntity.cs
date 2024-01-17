using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities;

/// <summary>
/// An implementation for GameObjects marked as CreateEmptyObject in the protobuf serializer.
/// They seem to represent unique (which is why they don't exist as a prefab) statically created objects which are part of the decor but can't be interacted with.
/// </summary>
/// <remarks>
/// The associated GameObjects are mostly alone in their own EntityCell.
/// To avoid creating an EntityCell entity for each of them we give it a global position and its actual cell reference.
/// It is safe to give a static cell because those objects don't move.
/// </remarks>
[Serializable]
[DataContract]
public class SerializedWorldEntity : WorldEntity
{
    public override AbsoluteEntityCell AbsoluteEntityCell => new(BatchId, CellId, Level);

    [DataMember(Order = 1)]
    public List<SerializedComponent> Components { get; set; } = new();

    [DataMember(Order = 2)]
    public int Layer { get; set; }

    /// <summary>
    /// This entity is not parented so it doesn't have info for the cell containing it,
    /// therefore we need to serialize it instead of generating it from a local position (which would make no sense)
    /// </summary>
    [DataMember(Order = 3)]
    public NitroxInt3 BatchId { get; set; }

    /// <inheritdoc cref="BatchId"/>
    [DataMember(Order = 4)]
    public NitroxInt3 CellId { get; set; }

    [IgnoreConstructor]
    protected SerializedWorldEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public SerializedWorldEntity(List<SerializedComponent> components, int layer, NitroxTransform transform, NitroxId id, NitroxId parentId, AbsoluteEntityCell cell)
    {
        Components = components;
        Layer = layer;
        Transform = transform;
        Id = id;
        ParentId = parentId;
        Level = cell.Level;
        BatchId = cell.BatchId;
        CellId = cell.CellId;
    }

    /// <remarks>Used for deserialization</remarks>
    public SerializedWorldEntity(List<SerializedComponent> components, int layer, NitroxInt3 batchId, NitroxInt3 cellId, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        Components = components;
        Layer = layer;
        BatchId = batchId;
        CellId = cellId;
    }

    public override string ToString()
    {
        return $"[{nameof(SerializedWorldEntity)} Components: {Components.Count}, Layer: {Layer}, BatchId: {BatchId}, CellId: {CellId} {base.ToString()}]";
    }
}
