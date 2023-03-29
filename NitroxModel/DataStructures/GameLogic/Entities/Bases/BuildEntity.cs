using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class BuildEntity : GlobalRootEntity
{
    // TODO: Move SavedBuild's fields in here
    [DataMember(Order = 1)]
    public SavedBuild SavedBuild { get; set; }

    [IgnoreConstructor]
    protected BuildEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public BuildEntity(SavedBuild savedBuild)
    {
        SavedBuild = savedBuild;
        Id = savedBuild.NitroxId;
    }

    /// <remarks>Used for deserialization</remarks>
    public BuildEntity(SavedBuild savedBuild, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        SavedBuild = savedBuild;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[BuildEntity {SavedBuild}]";
    }
}
