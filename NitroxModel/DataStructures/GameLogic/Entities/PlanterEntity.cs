using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
public class PlanterEntity : GlobalRootEntity
{
    [IgnoreConstructor]
    protected PlanterEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PlanterEntity(NitroxId id, NitroxId parentId)
    {
        Id = id;
        ParentId = parentId;
    }

    /// <remarks>
    /// Used for deserialization.
    /// <see cref="WorldEntity.SpawnedByServer"/> is set to true because this entity is meant to receive simulation locks
    /// </remarks>
    public PlanterEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, true, id, techType, metadata, parentId, childEntities) {}

    public override string ToString()
    {
        return $"[PlanterEntity {base.ToString()}]";
    }
}
