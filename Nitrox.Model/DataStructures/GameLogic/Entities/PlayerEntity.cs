using System;
using System.Collections.Generic;
using BinaryPack.Attributes;
using System.Runtime.Serialization;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Model.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class PlayerEntity : GlobalRootEntity
{
    [IgnoreConstructor]
    protected PlayerEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>
    /// Used for deserialization.
    /// <see cref="WorldEntity.SpawnedByServer"/> is set to true because this entity is meant to receive simulation locks
    /// </remarks>
    public PlayerEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, true, id, techType, metadata, parentId, childEntities) {}

    public override string ToString()
    {
        return $"[PlayerEntity {base.ToString()}]";
    }
}
