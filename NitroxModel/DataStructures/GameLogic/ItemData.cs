using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [DataContract]
    [ProtoInclude(50, typeof(BasicItemData))]
    [ProtoInclude(51, typeof(EquippedItemData))]
    [ProtoInclude(52, typeof(PlantableItemData))]
    public abstract class ItemData
    {
        [DataMember(Order = 1)]
        public NitroxId ContainerId { get; }

        [DataMember(Order = 2)]
        public NitroxId ItemId { get; }

        [DataMember(Order = 3)]
        public byte[] SerializedData { get; }

        [IgnoreConstructor]
        protected ItemData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public ItemData(NitroxId containerId, NitroxId itemId, byte[] serializedData)
        {
            ContainerId = containerId;
            ItemId = itemId;
            SerializedData = serializedData;
        }

        public override string ToString()
        {
            return $"[ItemData - ContainerId: {ContainerId}, Id: {ItemId}]";
        }
    }
}
