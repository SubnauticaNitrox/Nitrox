using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable]
[DataContract]
public class EscapePodWorldEntity : GlobalRootEntity
{
    [DataMember(Order = 1)]
    public bool Damaged { get; set; }

    [DataMember(Order = 2)]
    public List<ushort> Players { get; set; }

    [IgnoreConstructor]
    protected EscapePodWorldEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public EscapePodWorldEntity(NitroxVector3 position, NitroxId id, EntityMetadata metadata)
    {
        Id = id;
        Metadata = metadata;
        Transform = new NitroxTransform(position, NitroxQuaternion.Identity, NitroxVector3.Zero);
        Players = new List<ushort>();
        Level = 0;
        TechType = new NitroxTechType("EscapePod");
        Damaged = true;
        SpawnedByServer = true;

        ChildEntities = new List<Entity>();
    }

    /// <remarks>Used for deserialization</remarks>
    public EscapePodWorldEntity(bool damaged, List<ushort> players, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    {
        Damaged = damaged;
        Players = players;
    }

    public override string ToString()
    {
        return $"[EscapePodWorldEntity Damaged: {Damaged} {base.ToString()}]";
    }
}
