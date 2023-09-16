using System;
using BinaryPack.Attributes;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using System.Collections.Generic;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class VehicleWorldEntity : GlobalRootEntity
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

    public VehicleWorldEntity(NitroxId spawnerId, float constructionTime, NitroxTransform transform, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata) :
        base(transform, 0, classId, spawnedByServer, id, techType, metadata, null, new List<Entity>())
    {
        SpawnerId = spawnerId;
        ConstructionTime = constructionTime;
    }

    /// <remarks>Used for deserialization</remarks>
    public VehicleWorldEntity(NitroxId spawnerId, float constructionTime, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        SpawnerId = spawnerId;
        ConstructionTime = constructionTime;
    }

    public override string ToString()
    {
        return $"[VehicleEntity SpawnerId:{SpawnerId} ConstructionTime:{ConstructionTime} {base.ToString()}]";
    }
}
