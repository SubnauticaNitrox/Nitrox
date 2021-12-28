using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    [DynamicUnion]
    [ProtoContract]
    [ProtoInclude(50, typeof(EquippedItemData))]
    [ProtoInclude(51, typeof(PlantableItemData))]
    public class ItemData
    {
        [Index(-1)]
        [ProtoMember(1)]
        public virtual NitroxId ContainerId { get; protected set; }

        [Index(-2)]
        [ProtoMember(2)]
        public virtual NitroxId ItemId { get; protected set; }

        [Index(-3)]
        [ProtoMember(3)]
        public virtual byte[] SerializedData { get; protected set; } // TODO: change

        public ItemData()
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
            return "[ItemData ContainerId: " + ContainerId + "Id: " + ItemId + "]";
        }
    }
}
