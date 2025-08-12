using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class EscapePodEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public List<ushort> Players { get; set; } = [];

    [IgnoreConstructor]
    protected EscapePodEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public EscapePodEntity(NitroxVector3 position, NitroxId id, EntityMetadata metadata)
    {
        Transform = new NitroxTransform(position, NitroxQuaternion.Identity, NitroxVector3.One);
        Id = id;
        Metadata = metadata;
        Players = [];
        Level = 0;
        TechType = new NitroxTechType("EscapePod");
        SpawnedByServer = true;
        ChildEntities = [];
    }

    /// <remarks>Used for deserialization</remarks>
    public EscapePodEntity(List<ushort> players, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        Players = players;
    }

    public override string ToString()
    {
        return $"[EscapePodEntity Players: [{string.Join(", ", Players)}] {base.ToString()}]";
    }
}
