using System;
using System.Collections.Generic;
using System.Linq;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable]
[ProtoContract]
public class PlaceholderGroupWorldEntity : WorldEntity
{
    [IgnoreConstructor]
    protected PlaceholderGroupWorldEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }
    
    public PlaceholderGroupWorldEntity(WorldEntity worldEntity, List<PrefabPlaceholderEntity> prefabPlaceholders)
    {
        Id = worldEntity.Id;
        TechType = worldEntity.TechType;
        Metadata = worldEntity.Metadata;
        ParentId = worldEntity.ParentId;
        Transform = worldEntity.Transform;
        ChildEntities = worldEntity.ChildEntities;
        Level = worldEntity.Level;
        ClassId = worldEntity.ClassId;
        SpawnedByServer = worldEntity.SpawnedByServer;
        WaterParkId = worldEntity.WaterParkId;
        ExistsInGlobalRoot = worldEntity.ExistsInGlobalRoot;
        ChildEntities = prefabPlaceholders.Cast<Entity>().ToList();
    }
    
    /// <remarks>Used for deserialization</remarks>
    public PlaceholderGroupWorldEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId waterParkId, bool existsInGlobalRoot, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        Transform = transform;
        ChildEntities = childEntities;
        Level = level;
        ClassId = classId;
        SpawnedByServer = spawnedByServer;
        WaterParkId = waterParkId;
        ExistsInGlobalRoot = existsInGlobalRoot;
    }
}
