using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class Entity
    {
        public AbsoluteEntityCell AbsoluteEntityCell => new AbsoluteEntityCell((Position == new Vector3(0, 0, 0)) ? LocalPosition : Position, Level);

        [ProtoMember(1)]
        public Vector3 Position { get; set; }

        [ProtoMember(2)]
        public Quaternion Rotation { get; set; }

        [ProtoMember(3)]
        public Vector3 LocalScale { get; set; }

        [ProtoMember(4)]
        public Vector3 LocalPosition { get; set; }

        [ProtoMember(5)]
        public Quaternion LocalRotation { get; set; }

        [ProtoMember(6)]
        public TechType TechType { get; set; }

        [ProtoMember(7)]
        public string Guid { get; set; }

        [ProtoMember(8)]
        public int Level { get; set; }

        [ProtoMember(9)]
        public string ClassId { get; set; }

        [ProtoMember(10)]
        public List<Entity> ChildEntities { get; set; } = new List<Entity>();

        [ProtoMember(11)]
        public bool SpawnedByServer; // Keeps track if an entity was spawned by the server or a player
                                     // Server-spawned entities need to be techType white-listed to be simulated

        [ProtoMember(12)]
        public string WaterParkGuid { get; set; }

        [ProtoMember(13)]
        public byte[] SerializedGameObject { get; set; } // Some entities (such as dropped items) have already been serialized and include 
                                                         // special game object meta data (like battery charge)
        [ProtoMember(14)]
        public bool ExistsInGlobalRoot { get; set; }

        [ProtoMember(15)]
        public bool IsChild { get; set; }
                
        public Entity()
        {
            // Default Constructor for serialization
        }

        public Entity(Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            TechType techType,
            int level,
            string classId,
            bool spawnedByServer,
            string guid)
        {
            LocalScale = localScale;
            LocalPosition = localPosition;
            LocalRotation = localRotation;
            TechType = techType;
            Guid = guid;
            Level = level;
            ClassId = classId;
            SpawnedByServer = spawnedByServer;
            WaterParkGuid = null;
            SerializedGameObject = null;
            ExistsInGlobalRoot = false;
        }

        public Entity(Vector3 position, Quaternion rotation, Vector3 scale, TechType techType, int level, string classId, bool spawnedByServer, string waterParkGuid, byte[] serializedGameObject, bool existsInGlobalRoot, string guid)
        {
            Position = position;
            Rotation = rotation;
            LocalScale = scale;
            TechType = techType;
            Guid = guid;
            Level = level;
            ClassId = classId;
            SpawnedByServer = spawnedByServer;
            WaterParkGuid = waterParkGuid;
            SerializedGameObject = serializedGameObject;
            ExistsInGlobalRoot = existsInGlobalRoot;
        }

        public override string ToString()
        {
            return "[Entity Position: " + Position + " Local Position: " + LocalPosition + " Rotation: " + Rotation + " Local Rotation: " + LocalRotation + " TechType: " + TechType + " Guid: " + Guid + " Level: " + Level + " classId: " + ClassId + " ChildEntities: { " + string.Join(", ", ChildEntities.Select(c => c.ToString()).ToArray()) + " } SpawnedByServer: " + SpawnedByServer + "]";
        }
    }
}
