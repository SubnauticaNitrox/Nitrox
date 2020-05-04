using ProtoBufNet;
using System;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    [ProtoInclude(50, typeof(EquippedItemData))]
    public class ItemData
    {
        [ProtoMember(1)]
        public NitroxId ContainerId { get; }

        [ProtoMember(2)]
        public NitroxId ItemId { get; }

        [ProtoMember(3)]
        public byte[] SerializedData { get; }

        public ItemData()
        {
            // For serialization
        }

        [Nitrox.Newtonsoft.Json.JsonConstructor]
        public ItemData(NitroxId containerId, NitroxId itemId, byte[] serializedData)
        {
            ContainerId = containerId;
            ItemId = itemId;
            SerializedData = serializedData;
        }

        public override string ToString()
        {
            return "[ItemData ContainerId: " + ContainerId + "Id: " + ItemId + "]";
        }
    }
}
