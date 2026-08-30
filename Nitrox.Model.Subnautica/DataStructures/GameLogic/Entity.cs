using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic
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
        public List<EntityMetadata>? Metadata { get; set; }

        [DataMember(Order = 4)]
        public NitroxId? ParentId { get; set; }

        [DataMember(Order = 5)]
        public List<Entity> ChildEntities { get; set; } = [];

        [IgnoreConstructor]
        protected Entity()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public T? GetMetadata<T>() where T : EntityMetadata
        {
            if (Metadata != null)
            {
                foreach (EntityMetadata entry in Metadata)
                {
                    if (entry is T typed)
                    {
                        return typed;
                    }
                }
            }
            return null;
        }

        public bool TryGetMetadata<T>(out T metadata) where T : EntityMetadata
        {
            metadata = GetMetadata<T>();
            return metadata != null;
        }
        
        public void SetMetadata(EntityMetadata metadata)
        {
            Metadata ??= [];
            for (int i = 0; i < Metadata.Count; i++)
            {
                if (Metadata[i].GetType() == metadata.GetType())
                {
                    Metadata[i] = metadata;
                    return;
                }
            }
            Metadata.Add(metadata);
        }

        public override string ToString()
        {
            return $"[Entity id: {Id} techType: {TechType} Metadata: {(Metadata != null ? string.Join(", ", Metadata) : null)} ParentId: {ParentId} ChildEntities: {string.Join(",\n ", ChildEntities)}]";
        }
    }
}
