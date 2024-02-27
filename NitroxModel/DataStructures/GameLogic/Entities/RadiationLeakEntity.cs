using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
public class RadiationLeakEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public int ObjectIndex { get; set; }

    [IgnoreConstructor]
    protected RadiationLeakEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public RadiationLeakEntity(NitroxId id, int objectIndex, RadiationMetadata metadata)
    {
        Id = id;
        ObjectIndex = objectIndex;
        Metadata = metadata;
        Transform = new();
    }

    /// <remarks>Used for deserialization</remarks>
    public RadiationLeakEntity(int objectIndex, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        ObjectIndex = objectIndex;
    }
}
