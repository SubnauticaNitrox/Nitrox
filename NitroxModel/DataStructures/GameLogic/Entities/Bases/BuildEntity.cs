using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class BuildEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public NitroxVector3 Position;

    [DataMember(Order = 2)]
    public NitroxQuaternion Rotation;

    [DataMember(Order = 3)]
    public NitroxVector3 LocalScale;

    [DataMember(Order = 4)]
    public SavedBase SavedBase;

    // TODO: Add an operationId to easily detect desyncs

    [IgnoreConstructor]
    protected BuildEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public static BuildEntity MakeEmpty()
    {
        return new();
    }

    public BuildEntity(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, NitroxVector3 localScale, SavedBase savedBase)
    {
        Id = id;
        Position = position;
        Rotation = rotation;
        LocalScale = localScale;
        SavedBase = savedBase;

        Transform = new();
    }

    /// <remarks>Used for deserialization</remarks>
    public BuildEntity(SavedBase savedBase, NitroxVector3 position, NitroxQuaternion rotation, NitroxVector3 localScale, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        SavedBase = savedBase;
        Position = position;
        Rotation = rotation;
        LocalScale = localScale;

        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        Transform = transform;
        ChildEntities = childEntities;
        Level = level;
        ClassId = classId;
        SpawnedByServer = spawnedByServer;
    }

    public override string ToString()
    {
        return $"[BuildEntity Id: {Id}]";
    }
}
