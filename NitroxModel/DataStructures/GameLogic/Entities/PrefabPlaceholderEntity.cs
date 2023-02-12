using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxModel.DataStructures.GameLogic.Entities
{
    [Serializable]
    [DataContract]
    public class PrefabPlaceholderEntity : Entity
    {
        [DataMember(Order = 1)]
        public string ClassId { get; set; }

        [IgnoreConstructor]
        protected PrefabPlaceholderEntity()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public PrefabPlaceholderEntity(NitroxId id, NitroxTechType techType, NitroxId parentId)
        {
            Id = id;
            TechType = techType;
            ParentId = parentId;
            ChildEntities = new List<Entity>();
        }

        public PrefabPlaceholderEntity(NitroxId id, string classId, NitroxTechType techType, NitroxId parentId, List<Entity> childEntities)
        {
            Id = id;
            ClassId = classId;
            TechType = techType;
            ParentId = parentId;
            ChildEntities = childEntities;
        }

        /// <remarks>Used for deserialization</remarks>
        public PrefabPlaceholderEntity(NitroxId id, string classId, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
        {
            Id = id;
            ClassId = classId;
            TechType = techType;
            Metadata = metadata;
            ParentId = parentId;
            ChildEntities = childEntities;
        }
    }
}
