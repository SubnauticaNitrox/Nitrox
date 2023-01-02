using System;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities
{
    /*
     * A PrefabChildEntity is a gameobject that resides inside of a spawned prefab.  Although the server knows about these,
     * it is too cost prohibitive for it to send spawn data for all of these.  Instead, we let the game spawn them and tag
     * the entity after the fact.  An example of this is a keypad in the aurora; there is an overarching Door prefab with 
     * the keypad baked in - we simply update the id of the keypad on spawn.  Each PrefabChildEntity will always bubble up
     * to a root WorldEntity.
     */
    [Serializable]
    [DataContract]
    public class PrefabChildEntity : Entity
    {       
        [DataMember(Order = 1)]
        public int ComponentIndex { get; set; }

        [DataMember(Order = 2)]
        public string ClassId { get; set; }

        [IgnoreConstructor]
        protected PrefabChildEntity()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public PrefabChildEntity(NitroxId id, string classId, NitroxTechType techType, int componentIndex, EntityMetadata metadata, NitroxId parentId)
        {
            Id = id;
            TechType = techType;
            ComponentIndex = componentIndex;
            ParentId = parentId;
            ClassId = classId;
            Metadata = metadata;
        }

        /// <remarks>Used for deserialization</remarks>
        public PrefabChildEntity(int componentIndex, string classId, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
        {
            ComponentIndex = componentIndex;
            ClassId = classId;
            Id = id;
            TechType = techType;
            Metadata = metadata;
            ParentId = parentId;
            ChildEntities = childEntities;
        }

        public override string ToString()
        {
            return $"[PrefabChildEntity ComponentIndex: {ComponentIndex} ClassId: {ClassId} {base.ToString()}]";
        }
    }
}
