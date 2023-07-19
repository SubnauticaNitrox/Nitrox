using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class BuildEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public NitroxVector3 LocalPosition;

    [DataMember(Order = 2)]
    public NitroxQuaternion LocalRotation;

    [DataMember(Order = 3)]
    public NitroxVector3 LocalScale;

    [DataMember(Order = 4)]
    public BaseData BaseData;

    [IgnoredMember]
    public int OperationId;

    [IgnoreConstructor]
    protected BuildEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public static BuildEntity MakeEmpty()
    {
        return new();
    }

    public BuildEntity(NitroxId id, NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 localScale, BaseData baseData)
    {
        Id = id;
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        LocalScale = localScale;
        BaseData = baseData;

        Transform = new();
    }

    /// <remarks>Used for deserialization</remarks>
    public BuildEntity(BaseData baseData, NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 localScale, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        BaseData = baseData;
        LocalPosition = localPosition;
        LocalRotation = localRotation;
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
