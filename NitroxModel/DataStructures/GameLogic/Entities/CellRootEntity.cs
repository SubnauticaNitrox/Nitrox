using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class CellRootEntity : WorldEntity
{
    public static readonly string CLASS_ID = "55d7ab35-de97-4d95-af6c-ac8d03bb54ca";

    [IgnoreConstructor]
    protected CellRootEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CellRootEntity(NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 scale, NitroxTechType techType, int level, string classId, bool spawnedByServer, NitroxId id)
    {
        Transform = new NitroxTransform(localPosition, localRotation, scale);
        TechType = techType;
        Id = id;
        Level = level;
        ClassId = classId;
        SpawnedByServer = spawnedByServer;
    }

    /// <remarks>Used for deserialization</remarks>
    public CellRootEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
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
        return $"[CellRootEntity {base.ToString()}]";
    }
}
