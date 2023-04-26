using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class PlayerMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public List<EquippedItem> EquippedItems { get; }

    [IgnoreConstructor]
    protected PlayerMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PlayerMetadata(List<EquippedItem> equippedItems)
    {
        EquippedItems = equippedItems;
    }

    public override string ToString()
    {
        return $"[PlayerMetadata EquippedItems: {string.Join(",", EquippedItems)}]";
    }

    [Serializable]
    [DataContract]
    public class EquippedItem
    {
        [DataMember(Order = 1)]
        public NitroxId Id { get; }

        [DataMember(Order = 2)]
        public string Slot { get; }

        [DataMember(Order = 3)]
        public NitroxTechType TechType { get; }

        [IgnoreConstructor]
        protected EquippedItem()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public EquippedItem(NitroxId id, string slot, NitroxTechType techType)
        {
            Id = id;
            Slot = slot;
            TechType = techType;
        }
        public override string ToString()
        {
            return $"[EquippedItem Id: {Id} Slot: {Slot} TechType: {TechType}]";
        }
    }
}
