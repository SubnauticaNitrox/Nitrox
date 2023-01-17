using System;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities
{

    [Serializable]
    [DataContract]
    public class EscapePodWorldEntity : WorldEntity
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
            WaterParkId = null;
            ExistsInGlobalRoot = true;

            ChildEntities = new List<Entity>();
        }

        /// <remarks>Used for deserialization</remarks>
        public EscapePodWorldEntity(bool damaged, List<ushort> players, NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId waterParkId, bool existsInGlobalRoot, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
        {
            Damaged = damaged;
            Players = players;
            Id = id;
            TechType = techType;
            Metadata = metadata;
            ParentId = parentId;
            Transform = transform;
            ChildEntities = childEntities;
            Level = level;
            ClassId = classId;
            SpawnedByServer = spawnedByServer;
            WaterParkId = waterParkId;
            ExistsInGlobalRoot = existsInGlobalRoot;
        }

        public override string ToString()
        {
            return $"[EscapePodWorldEntity Damaged: {Damaged} {base.ToString()}]";
        }

    }
}
