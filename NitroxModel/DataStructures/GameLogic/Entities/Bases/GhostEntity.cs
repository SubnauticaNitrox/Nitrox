using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Bases;

[Serializable, DataContract]
public class GhostEntity : ModuleEntity
{
    [DataMember(Order = 1)]
    public NitroxBaseFace BaseFace { get; set; }

    [DataMember(Order = 2)]
    public BaseData BaseData { get; set; }

    [IgnoreConstructor]
    protected GhostEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public new static GhostEntity MakeEmpty()
    {
        return new GhostEntity();
    }

    /// <remarks>Used for deserialization</remarks>
    public GhostEntity(NitroxBaseFace baseFace, BaseData baseData, float constructedAmount, bool isInside, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(constructedAmount, isInside, transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        BaseFace = baseFace;
        BaseData = baseData;
    }

    public override string ToString()
    {
        return $"[GhostEntity Id: {Id}, ParentId: {ParentId}, ClassId: {ClassId}, Metadata: {Metadata}, ConstructedAmount: {ConstructedAmount}, IsInside: {IsInside}, BaseFace: [{BaseFace}], BaseData: {BaseData}]";
    }
}
