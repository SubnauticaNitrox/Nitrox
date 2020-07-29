using System;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class Entity : NitroxBehavior
    {
        public AbsoluteEntityCell AbsoluteEntityCell => new AbsoluteEntityCell(Transform.Position, Level);

        [ProtoMember(2)]
        public NitroxTechType TechType { get; set; }

        [ProtoMember(4)]
        public int Level { get; set; }

        [ProtoMember(5)]
        public string ClassId { get; set; }

        /// <summary>
        ///     Keeps track if an entity was spawned by the server or a player
        ///     Server-spawned entities need to be techType white-listed to be simulated
        /// </summary>
        [ProtoMember(6)]
        public bool SpawnedByServer;

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

        [ProtoMember(11)]
        public EntityMetadata Metadata { get; set; }

        // If set, this entity already exists as a gameobject in the world (maybe as a child of a prefab we already spawned).  This
        // id can be used to find the object and update the corresponding id.
        [ProtoMember(12)]
        public int? ExistingGameObjectChildIndex { get; set; }

        protected Entity()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public Entity(NitroxTechType techType, int level, string classId, bool spawnedByServer, int? existingGameObjectChildIndex)
        {
            TechType = techType;
            Level = level;
            ClassId = classId;
            SpawnedByServer = spawnedByServer;
            WaterParkId = null;
            SerializedGameObject = null;
            Metadata = null;
            ExistsInGlobalRoot = false;
            ExistingGameObjectChildIndex = existingGameObjectChildIndex;
        }

        public Entity(NitroxTechType techType, int level, string classId, bool spawnedByServer, NitroxId waterParkId, byte[] serializedGameObject, bool existsInGlobalRoot)
        {
            TechType = techType;
            Level = level;
            ClassId = classId;
            SpawnedByServer = spawnedByServer;
            WaterParkId = waterParkId;
            SerializedGameObject = serializedGameObject;
            ExistsInGlobalRoot = existsInGlobalRoot;
            ExistingGameObjectChildIndex = null;
        }

        protected Entity(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            TechType = (NitroxTechType)info.GetValue("techType", typeof(NitroxTechType));
            Level = info.GetInt32("level");
            ClassId = info.GetString("ClassId");
            WaterParkId = (NitroxId)info.GetValue("WaterParkId", typeof(NitroxId));
            SerializedGameObject = (byte[])info.GetValue("serializedGameObject", typeof(byte[]));
            ExistsInGlobalRoot = info.GetBoolean("existsInGlobalRoot");
            ExistingGameObjectChildIndex = (int?)info.GetValue("existingGameObjectChildIndex", typeof(int?));
            Metadata = (EntityMetadata)info.GetValue("metaData", typeof(EntityMetadata));
            SpawnedByServer = info.GetBoolean("spawnedByServer");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("techType", TechType);
            info.AddValue("level", Level);
            info.AddValue("ClassId", ClassId);
            info.AddValue("WaterParkId", WaterParkId);
            info.AddValue("serializedGameObject", SerializedGameObject);
            info.AddValue("existsInGlobalRoot", ExistsInGlobalRoot);
            info.AddValue("existingGameObjectChildIndex", ExistingGameObjectChildIndex);
            info.AddValue("metaData", Metadata);
            info.AddValue("spawnedByServer", SpawnedByServer);
        }

        public override string ToString()
        {
            return "[Entity Transform: " + Transform + " TechType: " + TechType + " Id: " + Id + " Level: " + Level + " classId: " + ClassId + " SpawnedByServer: " + SpawnedByServer + " ExistingGameObjectChildIndex: " + ExistingGameObjectChildIndex + "]";
        }
    }
}
