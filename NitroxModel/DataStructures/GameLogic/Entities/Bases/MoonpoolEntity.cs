using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class MoonpoolEntity : GlobalRootEntity
{
    // TODO: Move MoonpoolEntity's fields in here
    [DataMember(Order = 1)]
    public SavedMoonpool SavedMoonpool { get; set; }

    [IgnoreConstructor]
    protected MoonpoolEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public MoonpoolEntity(SavedMoonpool savedMoonpool, NitroxId parentId)
    {
        SavedMoonpool = savedMoonpool;
        Id = savedMoonpool.NitroxId;
        ParentId = parentId;
        //Metadata = savedMoonpool.Metadata;

        Transform = new();
    }

    /// <remarks>Used for deserialization</remarks>
    public MoonpoolEntity(SavedMoonpool savedMoonpool, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        SavedMoonpool = savedMoonpool;

        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        Transform = transform;
        ChildEntities = childEntities;
        Level = level;
        ClassId = classId;
        SpawnedByServer = spawnedByServer;
    }

    public override string ToString()
    {
        return $"[MoonpoolEntity {SavedMoonpool}]";
    }
}
