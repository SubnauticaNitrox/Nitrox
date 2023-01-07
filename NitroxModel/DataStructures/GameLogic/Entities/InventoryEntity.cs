using System;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using System.Collections.Generic;
using BinaryPack.Attributes;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities;

/// <summary>
/// Represents an object that can hold InventoryItemEntity, such as the locker in the escape pod.
/// </summary>
[Serializable]
[DataContract]
public class InventoryEntity : Entity
{
    [DataMember(Order = 1)]
    public int ComponentIndex { get; set; }

    [IgnoreConstructor]
    protected InventoryEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public InventoryEntity(int componentIndex, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        ComponentIndex = componentIndex;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[InventoryEntity {base.ToString()}]";
    }
}
