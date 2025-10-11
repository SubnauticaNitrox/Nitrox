using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Model.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
public class ReefbackEntity : WorldEntity
{
    [DataMember(Order = 1)]
    public int GrassIndex { get; set; }

    [DataMember(Order = 2)]
    public NitroxVector3 OriginalPosition { get; set; }

    [IgnoreConstructor]
    protected ReefbackEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public ReefbackEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities, int grassIndex, NitroxVector3 originalPosition) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        GrassIndex = grassIndex;
        OriginalPosition = originalPosition;
    }

    public override string ToString()
    {
        return $"[{nameof(ReefbackEntity)} GrassIndex: {GrassIndex}, OriginalPosition: {OriginalPosition} {base.ToString()}]";
    }
}
