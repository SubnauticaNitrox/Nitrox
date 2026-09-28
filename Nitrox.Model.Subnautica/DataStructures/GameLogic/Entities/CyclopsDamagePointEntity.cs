using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
public class CyclopsDamagePointEntity : Entity
{
    [DataMember(Order = 1)]
    public int DamagePointIndex { get; set; }

    [IgnoreConstructor]
    protected CyclopsDamagePointEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CyclopsDamagePointEntity(int damagePointIndex, NitroxId id, EntityMetadata? metadata, NitroxId parentId)
    {
        DamagePointIndex = damagePointIndex;
        Id = id;
        Metadata = metadata;
        ParentId = parentId;
    }

    /// <remarks>Used for deserialization</remarks>
    public CyclopsDamagePointEntity(int damagePointIndex, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        DamagePointIndex = damagePointIndex;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[CyclopsFireEntity DamagePointIndex: {DamagePointIndex}]";
    }
}
