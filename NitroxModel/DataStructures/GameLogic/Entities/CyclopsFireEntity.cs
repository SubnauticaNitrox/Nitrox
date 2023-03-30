using System;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class CyclopsFireEntity : Entity
{
    [DataMember(Order = 1)]
    public int RoomIndex { get; set; }

    [DataMember(Order = 2)]
    public int NodeIndex { get; set; }

    [IgnoreConstructor]
    protected CyclopsFireEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public CyclopsFireEntity(int roomIndex, int nodeIndex, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        RoomIndex = roomIndex;
        NodeIndex = nodeIndex;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[CyclopsFireEntity RoomIndex:{RoomIndex} NodeIndex:{NodeIndex} {base.ToString()}]";
    }
}
