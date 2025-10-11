using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Model.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class PathBasedChildEntity : Entity
{       
    [DataMember(Order = 1)]
    public string Path { get; set; }

    [IgnoreConstructor]
    protected PathBasedChildEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public PathBasedChildEntity(string path, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        Path = path;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[PathBasedChildEntity Path: {Path} {base.ToString()}]";
    }
}
