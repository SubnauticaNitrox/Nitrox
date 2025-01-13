using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities;

/// <summary>
/// Represents a Placeholder GameObject located under a PrefabPlaceholdersGroup
/// </summary>
[Serializable, DataContract]
public class PrefabPlaceholderEntity : WorldEntity
{
    [DataMember(Order = 1)]
    public int ComponentIndex { get; set; }

    [DataMember(Order = 2)]
    public bool IsEntitySlotEntity { get; set; }

    [IgnoreConstructor]
    protected PrefabPlaceholderEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PrefabPlaceholderEntity(WorldEntity worldEntity, bool isEntitySlotEntity, int componentIndex = 0)
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
        IsEntitySlotEntity = isEntitySlotEntity;
    }


    /// <remarks>Used for deserialization</remarks>
    public PrefabPlaceholderEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities, bool isEntitySlotEntity, int componentIndex) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        ComponentIndex = componentIndex;
        IsEntitySlotEntity = isEntitySlotEntity;
    }

    public override string ToString()
    {
        return $"[PrefabPlaceholderEntity ComponentIndex: {ComponentIndex} {base.ToString()}]";
    }
}
