using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class ModuleEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public NitroxVector3 LocalPosition;

    [DataMember(Order = 2)]
    public NitroxQuaternion LocalRotation;

    [DataMember(Order = 3)]
    public NitroxVector3 LocalScale;

    [DataMember(Order = 4)]
    public float ConstructedAmount;

    [DataMember(Order = 5)]
    public bool IsInside;

    [IgnoreConstructor]
    protected ModuleEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public static ModuleEntity MakeEmpty()
    {
        return new();
    }

    public ModuleEntity(NitroxId id, NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 localScale, float constructedAmount, bool isInside, NitroxId parentId = null)
    {
        Id = id;
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        LocalScale = localScale;
        ConstructedAmount = constructedAmount;
        IsInside = isInside;
        ParentId = parentId;

        Transform = new();
    }

    /// <remarks>Used for deserialization</remarks>
    public ModuleEntity(NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 localScale, float constructedAmount, bool isInside, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        LocalScale = localScale;
        ConstructedAmount = constructedAmount;
        IsInside = isInside;

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
        return $"[ModuleEntity Id: {Id}, ParentId: {ParentId}, ClassId: {ClassId}, LocalPosition: {LocalPosition}, LocalRotation: {LocalRotation}, LocalScale: {LocalScale}, ConstructedAmount: {ConstructedAmount}, IsInside: {IsInside}]";
    }
}
