using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [DataContract]
    public class EquippedItemData : ItemData
    {
        [DataMember(Order = 1)]
        public string Slot { get; }

        [DataMember(Order = 2)]
        public NitroxTechType TechType { get; }

        [IgnoreConstructor]
        protected EquippedItemData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public EquippedItemData(NitroxId containerId, NitroxId itemId, byte[] serializedData, string slot, NitroxTechType techType) : base(containerId, itemId, serializedData)
        {
            Slot = slot;
            TechType = techType;
        }

        public override string ToString()
        {
            return $"[EquippedItemData ContainerGuid: {ContainerId}Id: {ItemId} Slot: {Slot} TechType: {TechType}]";
        }
    }
}
