using System;
using BinaryPack.Attributes;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    [ProtoInclude(50, typeof(BasicItemData))]
    [ProtoInclude(51, typeof(EquippedItemData))]
    [ProtoInclude(52, typeof(PlantableItemData))]
    public abstract class ItemData
    {
        [ProtoMember(1)]
        public NitroxId ContainerId { get; }

        [ProtoMember(2)]
        public NitroxId ItemId { get; }

        [ProtoMember(3)]
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
