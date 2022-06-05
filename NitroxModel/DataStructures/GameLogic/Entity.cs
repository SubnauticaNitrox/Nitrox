using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class Entity
{
    [JsonMemberTransition]
    public NitroxTransform Transform { get; set; }

    [JsonMemberTransition]
    public NitroxTechType TechType { get; set; }

    [JsonMemberTransition]
    public NitroxId Id { get; set; }

    [JsonMemberTransition]
    public int Level { get; set; }

    /// <summary>
    ///     Gets the prefab class id assigned by Unity Engine. This is a unique <see cref="Guid"/>. 
    /// </summary>
    /// <remarks>
    ///     <a href="https://docs.unity3d.com/Manual/Prefabs.html">What is a prefab?</a>
    /// </remarks>
    [JsonMemberTransition]
    public string ClassId { get; set; }

    /// <summary>
    ///     Keeps track if an entity was spawned by the server or a player
    ///     Server-spawned entities need to be techType white-listed to be simulated
    /// </summary>
    [JsonMemberTransition] public bool SpawnedByServer;

    [JsonMemberTransition]
    public NitroxId WaterParkId { get; set; }

    /// <summary>
    ///     Gets or sets the the serialized GameObject for this entity which is used on the client-side to spawn it.
    /// </summary>
    /// <remarks>
    ///     Used for player droppable items including items that hold metadata/state that a player can change and should be persisted on the server.
    /// </remarks>
    [JsonMemberTransition]
    public byte[] SerializedGameObject { get; set; }

    [JsonMemberTransition]
    public bool ExistsInGlobalRoot { get; set; }

    [JsonMemberTransition]
    public NitroxId ParentId { get; set; }

    [JsonMemberTransition]
    public EntityMetadata Metadata { get; set; }

    /// <summary>
    ///     If set, this entity already exists as a gameobject in the world (maybe as a child of a prefab we already spawned).
    ///     This id can be used to find the object and update the corresponding id.
    /// </summary>
    [JsonMemberTransition]
    public int? ExistingGameObjectChildIndex { get; set; }

    public List<Entity> ChildEntities { get; set; } = new();

    public AbsoluteEntityCell AbsoluteEntityCell => new(Transform.Position, Level);

    // Constructor for serialization.
    private Entity() { }

    public Entity(NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 scale, NitroxTechType techType, int level, string classId, bool spawnedByServer, NitroxId id, int? existingGameObjectChildIndex, Entity parentEntity = null)
    {
        Transform = new NitroxTransform(localPosition, localRotation, scale, this);
        TechType = techType;
        Id = id;
        Level = level;
        ClassId = classId;
        SpawnedByServer = spawnedByServer;
        WaterParkId = null;
        SerializedGameObject = null;
        Metadata = null;
        ExistsInGlobalRoot = false;
        ExistingGameObjectChildIndex = existingGameObjectChildIndex;

        if (parentEntity != null)
        {
            ParentId = parentEntity.Id;
            Transform.SetParent(parentEntity.Transform, false);
        }
    }

    public Entity(NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 scale, NitroxTechType techType, int level, string classId, bool spawnedByServer, NitroxId waterParkId, byte[] serializedGameObject, bool existsInGlobalRoot,
                  NitroxId id)
    {
        Transform = new NitroxTransform(localPosition, localRotation, scale, this);
        TechType = techType;
        Id = id;
        Level = level;
        ClassId = classId;
        SpawnedByServer = spawnedByServer;
        WaterParkId = waterParkId;
        SerializedGameObject = serializedGameObject;
        ExistsInGlobalRoot = existsInGlobalRoot;
        ExistingGameObjectChildIndex = null;
    }

    public override string ToString()
    {
        return $"[Entity Transform: {Transform} TechType: {TechType} Id: {Id} Level: {Level} classId: {ClassId} ChildEntities.Count: {ChildEntities?.Count} SpawnedByServer: {SpawnedByServer} ExistingGameObjectChildIndex: {ExistingGameObjectChildIndex}]";
    }

    [OnDeserialized]
    private void JsonAfterDeserialization(StreamingContext context)
    {
        Transform.Entity = this;
    }
}
