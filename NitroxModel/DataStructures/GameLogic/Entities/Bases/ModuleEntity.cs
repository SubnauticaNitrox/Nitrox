using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

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

        Transform = new();
    }

    /// <remarks>Used for deserialization</remarks>
    public ModuleEntity(SavedModule savedModule, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        SavedModule = savedModule;

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
        return $"[ModuleEntity {SavedModule}]";
    }
}
