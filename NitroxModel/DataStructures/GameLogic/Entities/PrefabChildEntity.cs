using System;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using System.Collections.Generic;
using ProtoBufNet;
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
    [ProtoContract]
    public class PrefabChildEntity : Entity
    {       
        [ProtoMember(1)]
        public int ExistingGameObjectChildIndex { get; set; }

        [IgnoreConstructor]
        protected PrefabChildEntity()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public PrefabChildEntity(NitroxId id, NitroxTechType techType, int existingGameObjectChildIndex, Entity parent)
        {
            Id = id;
            TechType = techType;
            ExistingGameObjectChildIndex = existingGameObjectChildIndex;
            ParentId = parent.Id;
        }

        /// <remarks>Used for deserialization</remarks>
        public PrefabChildEntity(int existingGameObjectChildIndex, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
        {
            ExistingGameObjectChildIndex = existingGameObjectChildIndex;
            Id = id;
            TechType = techType;
            Metadata = metadata;
            ParentId = parentId;
            ChildEntities = childEntities;
        }
    }
}
