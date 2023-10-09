using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
[ProtoInclude(50, typeof(GhostEntity))]
public class ModuleEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public float ConstructedAmount { get; set; }

    [DataMember(Order = 2)]
    public bool IsInside { get; set; }

    [IgnoreConstructor]
    protected ModuleEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public static ModuleEntity MakeEmpty()
    {
        return new ModuleEntity();
    }

    /// <remarks>Used for deserialization</remarks>
    public ModuleEntity(float constructedAmount, bool isInside, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        ConstructedAmount = constructedAmount;
        IsInside = isInside;
    }

    public override string ToString()
    {
        return $"[ModuleEntity Id: {Id}, ParentId: {ParentId}, ClassId: {ClassId}, ConstructedAmount: {ConstructedAmount}, IsInside: {IsInside}]";
    }
}
