using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class InteriorPieceEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public NitroxBaseFace BaseFace;

    [IgnoreDataMember]
    public bool IsWaterPark => waterparkClassIds.Contains(ClassId);

    [IgnoreConstructor]
    protected InteriorPieceEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public static InteriorPieceEntity MakeEmpty()
    {
        return new();
    }

    public InteriorPieceEntity(NitroxId id, NitroxId parentId, NitroxBaseFace baseFace)
    {
        Id = id;
        ParentId = parentId;
        BaseFace = baseFace;

        Transform = new();
    }

    /// <remarks>Used for deserialization</remarks>
    public InteriorPieceEntity(NitroxBaseFace baseFace, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        BaseFace = baseFace;

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
        return $"[InteriorPieceEntity Id: {Id}, ParentId: {ParentId}, BaseFace: {BaseFace}]";
    }

    /// <summary>
    /// classIds for WaterPark.prefab and WaterParkLarge.prefab
    /// </summary>
    [IgnoreDataMember()]
    private readonly List<string> waterparkClassIds = new() { "31662630-7cba-4583-8456-2fa1c4cc31aa", "c2a91864-0f0f-4d8a-99b8-9867571763dd" };
}
