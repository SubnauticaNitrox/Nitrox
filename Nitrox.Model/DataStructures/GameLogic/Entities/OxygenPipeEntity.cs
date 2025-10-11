using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Model.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
public class OxygenPipeEntity : WorldEntity
{
    [DataMember(Order = 1)]
    public NitroxId ParentPipeId { get; set; }

    [DataMember(Order = 2)]
    public NitroxId RootPipeId { get; set; }

    [DataMember(Order = 3)]
    public NitroxVector3 ParentPosition { get; set; }

    [IgnoreConstructor]
    protected OxygenPipeEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public OxygenPipeEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities, NitroxId parentPipeId, NitroxId rootPipeId, NitroxVector3 parentPosition) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        ParentPipeId = parentPipeId;
        RootPipeId = rootPipeId;
        ParentPosition = parentPosition;
    }

    public override string ToString()
    {
        return $"[{nameof(OxygenPipeEntity)} ParentPipeId: {ParentPipeId}, RootPipeId: {RootPipeId}, ParentPosition: {ParentPosition} {base.ToString()}]";
    }
}
