using System;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using System.Collections.Generic;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.Unity;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class PlayerWorldEntity : GlobalRootEntity
{
    [IgnoreConstructor]
    protected PlayerWorldEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>
    /// Used for deserialization.
    /// <see cref="WorldEntity.SpawnedByServer"/> is set to true because this entity is meant to receive simulation locks
    /// </remarks>
    public PlayerWorldEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, true, id, techType, metadata, parentId, childEntities) {}

    public override string ToString()
    {
        return $"[PlayerEntity {base.ToString()}]";
    }
}
