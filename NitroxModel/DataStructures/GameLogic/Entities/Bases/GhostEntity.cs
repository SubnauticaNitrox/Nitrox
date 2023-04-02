using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class GhostEntity : GlobalRootEntity
{
    // TODO: Move SavedGhost's fields in here
    [DataMember(Order = 1)]
    public SavedGhost SavedGhost { get; set; }

    [IgnoreConstructor]
    protected GhostEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public GhostEntity(SavedGhost savedGhost, NitroxId parentId = null)
    {
        SavedGhost = savedGhost;
        Id = savedGhost.NitroxId;
        ParentId = parentId;
        //Metadata = savedGhost.Metadata;

        Transform = new();
    }

    /// <remarks>Used for deserialization</remarks>
    public GhostEntity(SavedGhost savedGhost, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        SavedGhost = savedGhost;

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
        return $"[GhostEntity {SavedGhost}]";
    }
}
