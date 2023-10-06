using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class PlaceholderGroupWorldEntity : WorldEntity
{
    [IgnoreConstructor]
    protected PlaceholderGroupWorldEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PlaceholderGroupWorldEntity(WorldEntity worldEntity, List<Entity> prefabPlaceholders)
    {
        Id = worldEntity.Id;
        TechType = worldEntity.TechType;
        Metadata = worldEntity.Metadata;
        ParentId = worldEntity.ParentId;
        Transform = worldEntity.Transform;
        Level = worldEntity.Level;
        ClassId = worldEntity.ClassId;
        SpawnedByServer = worldEntity.SpawnedByServer;
        ChildEntities = prefabPlaceholders;
    }

    /// <remarks>Used for deserialization</remarks>
    public PlaceholderGroupWorldEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        Transform = transform;
        Level = level;
        ClassId = classId;
        SpawnedByServer = spawnedByServer;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[PlaceholderGroupWorldEntity {base.ToString()}]";
    }
}
