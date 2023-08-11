using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class InteriorPieceEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public NitroxBaseFace BaseFace;

    [IgnoreDataMember]
    public bool IsWaterPark => ClassId is "31662630-7cba-4583-8456-2fa1c4cc31aa" or "c2a91864-0f0f-4d8a-99b8-9867571763dd"; // classIds for WaterPark.prefab and WaterParkLarge.prefab

    [IgnoreConstructor]
    protected InteriorPieceEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public static InteriorPieceEntity MakeEmpty()
    {
        return new InteriorPieceEntity();
    }

    /// <remarks>Used for deserialization</remarks>
    public InteriorPieceEntity(NitroxBaseFace baseFace, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        BaseFace = baseFace;
    }

    public override string ToString()
    {
        return $"[InteriorPieceEntity Id: {Id}, ParentId: {ParentId}, BaseFace: {BaseFace}]";
    }
}
