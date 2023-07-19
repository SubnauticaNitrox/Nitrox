using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class GhostEntity : ModuleEntity
{
    [DataMember(Order = 1)]
    public NitroxBaseFace BaseFace;

    [DataMember(Order = 2)]
    public BaseData BaseData;

    [IgnoreConstructor]
    protected GhostEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public static new GhostEntity MakeEmpty()
    {
        return new();
    }

    public GhostEntity(NitroxId id, NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 localScale, float constructedAmount, bool isInside, NitroxBaseFace baseFace, BaseData baseData, NitroxId parentId = null)
    {
        Id = id;
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        LocalScale = localScale;
        ConstructedAmount = constructedAmount;
        IsInside = isInside;
        BaseFace = baseFace;
        BaseData = baseData;
        ParentId = parentId;

        Transform = new();
    }

    /// <remarks>Used for deserialization</remarks>
    public GhostEntity(NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 localScale, float constructedAmount, bool isInside, NitroxBaseFace baseFace, BaseData baseData, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        LocalScale = localScale;
        ConstructedAmount = constructedAmount;
        IsInside = isInside;
        BaseFace = baseFace;
        BaseData = baseData;

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
        return $"[GhostEntity Id: {Id}, ParentId: {ParentId}, ClassId: {ClassId}, LocalPosition: {LocalPosition}, LocalRotation: {LocalRotation}, LocalScale: {LocalScale}, ConstructedAmount: {ConstructedAmount}, IsInside: {IsInside}, BaseFace: [{BaseFace}], BaseData: {BaseData}";
    }
}
