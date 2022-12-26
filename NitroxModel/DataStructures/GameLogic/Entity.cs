using System;
using System.Collections.Generic;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    [ProtoInclude(50, typeof(WorldEntity))]
    [ProtoInclude(60, typeof(PrefabChildEntity))]
    [ProtoInclude(70, typeof(PrefabPlaceholderEntity))]
    public abstract class Entity
    {
        [ProtoMember(1)]
        public NitroxId Id { get; set; }

        [ProtoMember(2)]
        public NitroxTechType TechType { get; set; }

        [ProtoMember(3)]
        public EntityMetadata Metadata { get; set; }

        [ProtoMember(4)]
        public NitroxId ParentId { get; set; }

        [ProtoMember(5)]
        public List<Entity> ChildEntities { get; set; } = new List<Entity>();

        [IgnoreConstructor]
        protected Entity()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public override string ToString()
        {
            return $"[Entity id: {Id} techType: {TechType} Metadata: {Metadata} ParentId: {ParentId} ChildEntities: {string.Join(",\n ", ChildEntities)}]";
        }
    }
}
