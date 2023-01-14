using System;
using BinaryPack.Attributes;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using System.Collections.Generic;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class VehicleWorldEntity : WorldEntity
{
    [DataMember(Order = 1)]
    public NitroxId SpawnerId { get; set; }

    [DataMember(Order = 2)]
    public float ConstructionTime { get; set; }

    [IgnoreConstructor]
    protected VehicleWorldEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public VehicleWorldEntity(NitroxId spawnerId, float constructionTime, NitroxTransform transform, string classId, bool spawnedByServer,  NitroxId id, NitroxTechType techType, EntityMetadata metadata)
    {
        SpawnerId = spawnerId;
        ConstructionTime = constructionTime;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        Transform = transform;
        ChildEntities = new List<Entity>();
        Level = 0;
        ClassId = classId;
        SpawnedByServer = spawnedByServer;
        ExistsInGlobalRoot = true;
    }

    /// <remarks>Used for deserialization</remarks>
    public VehicleWorldEntity(NitroxId spawnerId, float constructionTime, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId waterParkId, bool existsInGlobalRoot, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        SpawnerId = spawnerId;
        ConstructionTime = constructionTime;
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

    public override string ToString()
    {
        return $"[VehicleEntity SpawnerId:{SpawnerId} ConstructionTime:{ConstructionTime} {base.ToString()}]";
    }
}
