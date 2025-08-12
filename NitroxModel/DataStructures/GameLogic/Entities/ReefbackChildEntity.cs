using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
public class ReefbackChildEntity : WorldEntity
{
    [DataMember(Order = 1)]
    public ReefbackChildType Type { get; set; }

    [IgnoreConstructor]
    protected ReefbackChildEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public ReefbackChildEntity(NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 scale, NitroxTechType techType, int level, string classId, bool spawnedByServer, NitroxId id, ReefbackEntity parentEntity, ReefbackChildType type) :
        base(localPosition, localRotation, scale, techType, level, classId, spawnedByServer, id, null)
    {
        Type = type;
        // Manually set ParentId instead of providing parentEntity to the base constructor to avoid having the Transform being parented
        ParentId = parentEntity.Id;
    }

    /// <remarks>Used for deserialization</remarks>
    public ReefbackChildEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities, ReefbackChildType type) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        Type = type;
    }

    public override string ToString()
    {
        return $"[{nameof(ReefbackChildEntity)} Type: {Type} {base.ToString()}]";
    }

    public enum ReefbackChildType
    {
        CREATURE,
        PLANT
    }
}
