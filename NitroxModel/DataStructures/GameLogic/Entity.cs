using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [DataContract]
    [ProtoInclude(50, typeof(PrefabChildEntity))]
    [ProtoInclude(51, typeof(InventoryEntity))]
    [ProtoInclude(52, typeof(InventoryItemEntity))]
    [ProtoInclude(53, typeof(PathBasedChildEntity))]
    [ProtoInclude(54, typeof(InstalledBatteryEntity))]
    [ProtoInclude(55, typeof(InstalledModuleEntity))]
    [ProtoInclude(56, typeof(WorldEntity))]
    [ProtoInclude(57, typeof(BaseLeakEntity))]
    public abstract class Entity
    {
        [DataMember(Order = 1)]
        public NitroxId Id { get; set; }

        [DataMember(Order = 2)]
        public NitroxTechType TechType { get; set; }

        [DataMember(Order = 3)]
        public EntityMetadata Metadata { get; set; }

        [DataMember(Order = 4)]
        public NitroxId ParentId { get; set; }

        [DataMember(Order = 5)]
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
