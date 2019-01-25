using System;
using UnityEngine;
using System.Collections.Generic;
using ProtoBuf;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class Entity
    {
        public AbsoluteEntityCell AbsoluteEntityCell => new AbsoluteEntityCell(Position, Level);

        [ProtoMember(1)]
        public Vector3 Position { get; set; }

        [ProtoMember(2)]
        public Quaternion Rotation { get; set; }

        [ProtoMember(3)]
        public Vector3 Scale { get; set; }

        [ProtoMember(4)]
        public TechType TechType { get; set; }

        [ProtoMember(5)]
        public string Guid { get; set; }

        [ProtoMember(6)]
        public int Level { get; set; }

        [ProtoMember(7)]
        public string ClassId { get; set; }

        [ProtoMember(8)]
        public List<Entity> ChildEntities { get; set; } = new List<Entity>();

        [ProtoMember(9)]
        public bool SpawnedByServer; // Keeps track if an entity was spawned by the server or a player
                                     // Server-spawned entities need to be techType white-listed to be simulated

        [ProtoMember(10)]
        public string WaterParkGuid { get; set; }

        [ProtoMember(11)]
        public byte[] SerializedGameObject { get; set; } // Some entities (such as dropped items) have already been serialized and include 
                                                         // special game object meta data (like battery charge)
        [ProtoMember(12)]
        public bool ExistsInGlobalRoot { get; set; }
                
        public Entity()
        {
            // Default Constructor for serialization
        }

        public Entity(Vector3 position, Quaternion rotation, Vector3 scale, TechType techType, int level, string classId, bool spawnedByServer, string guid)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
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
            Scale = scale;
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
            return "[Entity Position: " + Position + " TechType: " + TechType + " Guid: " + Guid + " Level: " + Level + " classId: " + ClassId + " ChildEntities: " + ChildEntities + " SpawnedByServer: " + SpawnedByServer + "]";
        }
    }
}
