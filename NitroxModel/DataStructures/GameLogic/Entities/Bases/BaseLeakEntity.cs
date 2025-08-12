using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class BaseLeakEntity : Entity
{
    [DataMember(Order = 1)]
    public float Health { get; set; }

    [DataMember(Order = 2)]
    public NitroxInt3 RelativeCell { get; set; }

    [IgnoreConstructor]
    protected BaseLeakEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public BaseLeakEntity(float health, NitroxInt3 relativeCell, NitroxId leakId, NitroxId parentId)
    {
        Health = health;
        RelativeCell = relativeCell;
        Id = leakId;
        ParentId = parentId;
    }

    /// <remarks>Used for deserialization</remarks>
    public BaseLeakEntity(float health, NitroxInt3 relativeCell, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        Health = health;
        RelativeCell = relativeCell;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[BaseLeakEntity Health: {Health}, RelativeCell: {RelativeCell}]";
    }
}
