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
        [ProtoMember(1)]
        public AbsoluteEntityCell AbsoluteEntityCell => new AbsoluteEntityCell(Position, Level);

        [ProtoMember(2)]
        public Vector3 Position { get; set; }
        
        [ProtoMember(3)]
        public Quaternion Rotation { get; set; }

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

        public Entity()
        {
            // Default Constructor for serialization
        }

        public Entity(Vector3 position, Quaternion rotation, TechType techType, int level, string classId, bool spawnedByServer)
        {
            Position = position;
            Rotation = rotation;
            TechType = techType;
            Guid = System.Guid.NewGuid().ToString();
            Level = level;
            ClassId = classId;
            SpawnedByServer = spawnedByServer;
        }

        public override string ToString()
        {
            return "[Entity Position: " + Position + " TechType: " + TechType + " Guid: " + Guid + " Level: " + Level + " classId: " + ClassId + " ChildEntities: " + ChildEntities + " SpawnedByServer: " + SpawnedByServer + "]";
        }
    }
}
