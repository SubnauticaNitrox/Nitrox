using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class InteriorPieceEntity : GlobalRootEntity
{
    // TODO: Move InteriorPieceEntity's fields in here
    [DataMember(Order = 1)]
    public SavedInteriorPiece SavedInteriorPiece { get; set; }

    [IgnoreConstructor]
    protected InteriorPieceEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public InteriorPieceEntity(SavedInteriorPiece savedInteriorPiece, NitroxId parentId)
    {
        SavedInteriorPiece = savedInteriorPiece;
        Id = savedInteriorPiece.NitroxId;
        ParentId = parentId;
        //Metadata = savedInteriorPiece.Metadata;
    }

    /// <remarks>Used for deserialization</remarks>
    public InteriorPieceEntity(SavedInteriorPiece savedInteriorPiece, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        SavedInteriorPiece = savedInteriorPiece;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[InteriorPieceEntity {SavedInteriorPiece}]";
    }
}
