using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
public class PlaceholderGroupWorldEntity : WorldEntity
{
    [DataMember(Order = 1)]
    public int ComponentIndex { get; set; }

    [IgnoreConstructor]
    protected PlaceholderGroupWorldEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PlaceholderGroupWorldEntity(WorldEntity worldEntity, int componentIndex = 0)
    {
        Id = worldEntity.Id;
        TechType = worldEntity.TechType;
        Metadata = worldEntity.Metadata;
        ParentId = worldEntity.ParentId;
        Transform = worldEntity.Transform;
        Level = worldEntity.Level;
        ClassId = worldEntity.ClassId;
        SpawnedByServer = worldEntity.SpawnedByServer;
        ChildEntities = worldEntity.ChildEntities;
        ComponentIndex = componentIndex;
    }

    /// <remarks>Used for deserialization</remarks>
    public PlaceholderGroupWorldEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities, int componentIndex) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        ComponentIndex = componentIndex;
    }

    public override string ToString()
    {
        return $"[PlaceholderGroupWorldEntity ComponentIndex: {ComponentIndex} {base.ToString()}]";
    }
}
