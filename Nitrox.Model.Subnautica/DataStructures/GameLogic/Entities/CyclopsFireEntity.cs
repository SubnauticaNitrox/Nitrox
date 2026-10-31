using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
public class CyclopsFireEntity : Entity
{
    [DataMember(Order = 1)]
    public CyclopsRooms Room { get; set; }

    [DataMember(Order = 2)]
    public int NodeIndex { get; set; }

    [IgnoreConstructor]
    protected CyclopsFireEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CyclopsFireEntity(CyclopsRooms room, int nodeIndex, NitroxId id, NitroxId parentId)
    {
        Room = room;
        NodeIndex = nodeIndex;
        Id = id;
        ParentId = parentId;
    }

    /// <remarks>Used for deserialization</remarks>
    public CyclopsFireEntity(CyclopsRooms room, int nodeIndex, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        Room = room;
        NodeIndex = nodeIndex;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[CyclopsFireEntity Room: {Room}, NodeIndex: {NodeIndex}]";
    }
}
