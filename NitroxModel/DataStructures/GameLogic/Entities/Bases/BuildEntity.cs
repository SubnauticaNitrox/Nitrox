using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class BuildEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public BaseData BaseData { get; set; }

    [IgnoredMember]
    public int OperationId;

    [IgnoreConstructor]
    protected BuildEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public static BuildEntity MakeEmpty()
    {
        return new BuildEntity();
    }

    /// <remarks>Used for deserialization</remarks>
    public BuildEntity(BaseData baseData, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        BaseData = baseData;
    }

    public override string ToString()
    {
        return $"[BuildEntity Id: {Id}]";
    }
}
