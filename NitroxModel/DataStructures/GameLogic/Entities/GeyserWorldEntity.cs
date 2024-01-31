using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
public class GeyserWorldEntity : WorldEntity
{
    [DataMember(Order = 1)]
    public float RandomIntervalVarianceMultiplier { get; set; }

    [DataMember(Order = 2)]
    public float StartEruptTime { get; set; }

    [IgnoreConstructor]
    protected GeyserWorldEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public GeyserWorldEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities, float randomIntervalVarianceMultiplier, float startEruptTime) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        RandomIntervalVarianceMultiplier = randomIntervalVarianceMultiplier;
        StartEruptTime = startEruptTime;
    }

    public override string ToString()
    {
        return $"[{nameof(GeyserWorldEntity)} RandomIntervalVarianceMultiplier: {RandomIntervalVarianceMultiplier}, StartEruptTime: {StartEruptTime} {base.ToString()}]";
    }
}
