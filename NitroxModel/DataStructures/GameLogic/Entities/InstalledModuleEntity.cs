using System;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using System.Collections.Generic;
using BinaryPack.Attributes;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class InstalledModuleEntity : Entity
{
    [DataMember(Order = 1)]
    public string Slot { get; set; }

    [DataMember(Order = 2)]
    public string ClassId { get; set; }

    [IgnoreConstructor]
    protected InstalledModuleEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public InstalledModuleEntity(string slot, string classId, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        Slot = slot;
        ClassId = classId;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[InstalledModuleEntity Slot: {Slot} ClassId: {ClassId} {base.ToString()}]";
    }
}
