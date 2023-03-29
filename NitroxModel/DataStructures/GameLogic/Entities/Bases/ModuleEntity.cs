using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class ModuleEntity : GlobalRootEntity
{
    // TODO: Move ModuleEntity's fields in here
    [DataMember(Order = 1)]
    public SavedModule SavedModule { get; set; }

    [IgnoreConstructor]
    protected ModuleEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public ModuleEntity(SavedModule savedModule, NitroxId parentId = null)
    {
        SavedModule = savedModule;
        Id = savedModule.NitroxId;
        ParentId = parentId;
    }

    /// <remarks>Used for deserialization</remarks>
    public ModuleEntity(SavedModule savedModule, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        SavedModule = savedModule;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[ModuleEntity {SavedModule}]";
    }
}
