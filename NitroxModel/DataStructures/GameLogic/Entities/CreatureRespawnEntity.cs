using System;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using System.Collections.Generic;
using BinaryPack.Attributes;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
public class CreatureRespawnEntity : WorldEntity
{
    [DataMember(Order = 1)]
    public float SpawnTime { get; set; }

    [DataMember(Order = 2)]
    public NitroxTechType RespawnTechType { get; set; }

    [DataMember(Order = 3)]
    public List<string> AddComponents { get; set; }

    [IgnoreConstructor]
    protected CreatureRespawnEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public CreatureRespawnEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities, float spawnTime, NitroxTechType respawnTechType, List<string> addComponents) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        SpawnTime = spawnTime;
        RespawnTechType = respawnTechType;
        AddComponents = addComponents;
    }

    public override string ToString()
    {
        return $"[{nameof(CreatureRespawnEntity)} SpawnTime: {SpawnTime}, RespawnTechType: {RespawnTechType}, AddComponents: [{string.Join(", ", AddComponents)}] {base.ToString()}]";
    }
}
