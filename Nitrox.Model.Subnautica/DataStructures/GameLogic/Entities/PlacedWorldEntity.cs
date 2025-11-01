using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
public class PlacedWorldEntity : WorldEntity
{
    [IgnoreConstructor]
    protected PlacedWorldEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public PlacedWorldEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities) { }
}
