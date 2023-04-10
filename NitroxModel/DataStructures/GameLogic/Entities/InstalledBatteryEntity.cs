using System;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class InstalledBatteryEntity : Entity
{       
    [IgnoreConstructor]
    protected InstalledBatteryEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public InstalledBatteryEntity(NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[InstalledBatteryEntity {base.ToString()}]";
    }
}
