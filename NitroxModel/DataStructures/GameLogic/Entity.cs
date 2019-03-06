using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;
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

        [ProtoIgnore]
        public Dictionary<string, Entity> ChildEntitiesByGuid { get { return childEntitiesByGuid; } set { childEntitiesByGuid = value; } }

        [NonSerialized]
        [ProtoIgnore]
        private Dictionary<string, Entity> childEntitiesByGuid;

        ///<summary>
        /// Used for Serialization don't use
        /// </summary>
        [ProtoMember(10)]
        public List<Entity> ChildEntities { get; set; }

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
            string guid,
            bool isChild)
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
            ChildEntitiesByGuid = new Dictionary<string, Entity>();
            IsChild = isChild;
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
            ChildEntitiesByGuid = new Dictionary<string, Entity>();
            IsChild = false;
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
            ChildEntitiesByGuid = new Dictionary<string, Entity>();
        }

        [OnDeserialized]
        private void OnDeserialize(StreamingContext streamingContext)
        {
            ChildEntitiesByGuid = new Dictionary<string, Entity>(); // Converts back to Dictionary
            if (ChildEntities != null)
            {
                foreach (Entity child in ChildEntities)
                {
                    ChildEntitiesByGuid.Add(child.Guid, child);
                }
            }
        }

        [OnSerializing]
        [ProtoBeforeSerialization]
        private void OnSerialize(StreamingContext streamingContext) // Converts Dictionary to List for serialization by our friend Windows
        {
            List<Entity> childList = ChildEntitiesByGuid.Values.ToList();
            if (childList == null)
            {
                childList = new List<Entity>();
            }

            ChildEntities = ChildEntitiesByGuid.Values.ToList();
        }

        public void AddChild(Entity child)
        {
            ChildEntitiesByGuid.Add(Guid, child);
        }

        public override string ToString()
        {
            List<Entity> childList = ChildEntitiesByGuid.Values.ToList();
            if (childList == null)
            {
                childList = new List<Entity>();
            }

            return "[Entity Position: " + Position + " Local Position: " + LocalPosition + " Rotation: " + Rotation + " Local Rotation: " + LocalRotation + " TechType: " + TechType + " Guid: " + Guid + " Level: " + Level + " classId: " + ClassId + " ChildEntities: { " + string.Join(", ", ChildEntities.Select(c => c.ToString()).ToArray()) + " } SpawnedByServer: " + SpawnedByServer + "]";
        }
    }
}
