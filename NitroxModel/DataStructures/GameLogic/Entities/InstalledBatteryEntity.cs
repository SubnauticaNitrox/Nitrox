using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class InstalledBatteryEntity : Entity
{
    [DataMember(Order = 1)]
    public int ComponentIndex { get; set; }

    [IgnoreConstructor]
    protected InstalledBatteryEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public InstalledBatteryEntity(int componentIndex, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
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
        return $"[InstalledBatteryEntity ComponentIndex: {ComponentIndex} {base.ToString()}]";
    }
}
