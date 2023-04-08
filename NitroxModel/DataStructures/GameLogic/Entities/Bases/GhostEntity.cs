using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class GhostEntity : ModuleEntity
{
    [DataMember(Order = 1)]
    public NitroxBaseFace BaseFace;

    [DataMember(Order = 2)]
    public SavedBase SavedBase;

    [IgnoreConstructor]
    protected GhostEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public static new GhostEntity MakeEmpty()
    {
        return new();
    }

    public GhostEntity(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, NitroxVector3 localScale, float constructedAmount, bool isInside, NitroxBaseFace baseFace, SavedBase savedBase, NitroxId parentId = null)
    {
        Id = id;
        Position = position;
        Rotation = rotation;
        LocalScale = localScale;
        ConstructedAmount = constructedAmount;
        IsInside = isInside;
        BaseFace = baseFace;
        SavedBase = savedBase;
        ParentId = parentId;

        Transform = new();
    }

    /// <remarks>Used for deserialization</remarks>
    public GhostEntity(NitroxVector3 position, NitroxQuaternion rotation, NitroxVector3 localScale, float constructedAmount, bool isInside, NitroxBaseFace baseFace, SavedBase savedBase, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        Position = position;
        Rotation = rotation;
        LocalScale = localScale;
        ConstructedAmount = constructedAmount;
        IsInside = isInside;
        BaseFace = baseFace;
        SavedBase = savedBase;

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
        return $"[GhostEntity Id: {Id}, ParentId: {ParentId}, ClassId: {ClassId}, Position: {Position}, Rotation: {Rotation}, LocalScale: {LocalScale}, ConstructedAmount: {ConstructedAmount}, IsInside: {IsInside}, BaseFace: [{BaseFace}], SavedBase: {SavedBase}";
    }
}
