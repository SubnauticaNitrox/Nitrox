using System;
using System.Collections.Generic;
using BinaryPack.Attributes;
using System.Runtime.Serialization;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Model.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class InventoryItemEntity : Entity
{
    [DataMember(Order = 1)]
    public string ClassId { get; set; }

    [IgnoreConstructor]
    protected InventoryItemEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public InventoryItemEntity(NitroxId id, string classId, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        ClassId = classId;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[InventoryItemEntity ClassId: {ClassId} {base.ToString()}]";
    }
}
