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

    [DataMember(Order = 2)]
    public float Constructed;

    [IgnoreConstructor]
    protected InteriorPieceEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public static InteriorPieceEntity MakeEmpty()
    {
        return new();
    }

    public InteriorPieceEntity(NitroxId id, NitroxId parentId, NitroxBaseFace baseFace, float constructed)
    {
        Id = id;
        ParentId = parentId;
        BaseFace = baseFace;
        Constructed = constructed;
        //Metadata = savedInteriorPiece.Metadata;

        Transform = new();
    }

    /// <remarks>Used for deserialization</remarks>
    public InteriorPieceEntity(NitroxBaseFace baseFace, float constructed, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        BaseFace = baseFace;
        Constructed = constructed;

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
        return $"[InteriorPieceEntity BaseFace: {BaseFace}, Constructed: {Constructed}]";
    }
}
