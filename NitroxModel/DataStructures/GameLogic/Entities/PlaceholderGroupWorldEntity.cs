using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class PlaceholderGroupWorldEntity : WorldEntity
{
    [DataMember(Order = 1)]
    public Entity[] PrefabPlaceholders { get; set; }

    public override List<Entity> ChildEntities => PrefabPlaceholders.Where(e => e != null).ToList();

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
        WaterParkId = worldEntity.WaterParkId;
        ExistsInGlobalRoot = worldEntity.ExistsInGlobalRoot;

        PrefabPlaceholders = prefabPlaceholders.ToArray();
    }

    /// <remarks>Used for deserialization</remarks>
    public PlaceholderGroupWorldEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId waterParkId, bool existsInGlobalRoot, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities, Entity[] prefabPlaceholders)
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
        PrefabPlaceholders = prefabPlaceholders;
    }
}
