using System;
using System.Collections.Generic;
using ProtoBufNet;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class Entity
    {
        /// <summary>
        ///     Keeps track if an entity was spawned by the server or a player
        ///     Server-spawned entities need to be techType white-listed to be simulated
        /// </summary>
        [ProtoMember(6)]
        public bool SpawnedByServer;

        public AbsoluteEntityCell AbsoluteEntityCell => new AbsoluteEntityCell(Transform.Position, Level);

        [ProtoMember(1)]
        public NitroxTransform Transform { get; set; }

        [ProtoMember(2)]
        public TechType TechType { get; set; }

        [ProtoMember(3)]
        public NitroxId Id { get; set; }

        [ProtoMember(4)]
        public int Level { get; set; }

        [ProtoMember(5)]
        public string ClassId { get; set; }

        [ProtoMember(7)]
        public NitroxId WaterParkId { get; set; }

        /// <summary>
        ///     Gets or sets the the serialized GameObject for this entity which is used on the client-side to spawn it.
        /// </summary>
        /// <remarks>
        ///     Used for player droppable items including items that hold metadata/state that a player can change and should be persisted on the server.
        /// </remarks>
        [ProtoMember(8)]
        public byte[] SerializedGameObject { get; set; }

        [ProtoMember(9)]
        public bool ExistsInGlobalRoot { get; set; }

        [ProtoMember(10)]
        public NitroxId ParentId { get; set; }

        public List<Entity> ChildEntities { get; set; } = new List<Entity>();

        public Entity()
        {
            // Default Constructor for serialization
        }

        public Entity(Vector3 localPosition, Quaternion localRotation, Vector3 scale, TechType techType, int level, string classId, bool spawnedByServer, NitroxId id, Entity parentEntity = null)
        {
            Transform = new NitroxTransform(localPosition, localRotation, scale, this);
            TechType = techType;
            Id = id;
            Level = level;
            ClassId = classId;
            SpawnedByServer = spawnedByServer;
            WaterParkId = null;
            SerializedGameObject = null;
            ExistsInGlobalRoot = false;

            if (parentEntity != null)
            {
                ParentId = parentEntity.Id;
                Transform.SetParent(parentEntity.Transform);
            }
        }

        public Entity(Vector3 position, Quaternion rotation, Vector3 scale, TechType techType, int level, string classId, bool spawnedByServer, NitroxId waterParkId, byte[] serializedGameObject, bool existsInGlobalRoot, NitroxId id)
        {
            Transform = new NitroxTransform(position, rotation, scale, this);
            TechType = techType;
            Id = id;
            Level = level;
            ClassId = classId;
            SpawnedByServer = spawnedByServer;
            WaterParkId = waterParkId;
            SerializedGameObject = serializedGameObject;
            ExistsInGlobalRoot = existsInGlobalRoot;
        }

        public override string ToString()
        {
            return "[Entity Transform: " + Transform + " TechType: " + TechType + " Id: " + Id + " Level: " + Level + " classId: " + ClassId + " ChildEntities: " + string.Join(",\n ", ChildEntities) + " SpawnedByServer: " + SpawnedByServer + "]";
        }

        [ProtoAfterDeserialization]
        private void ProtoAfterDeserialization()
        {
            Transform.Entity = this;
        }
    }
}
