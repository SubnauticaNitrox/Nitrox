using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class ItemContainerRemove : Packet
    {
        [Index(0)]
        public virtual NitroxId OwnerId { get; protected set; }
        [Index(1)]
        public virtual NitroxId ItemId { get; protected set; }

        private ItemContainerRemove() { }

        public ItemContainerRemove(NitroxId ownerId, NitroxId itemId)
        {
            OwnerId = ownerId;
            ItemId = itemId;
        }
    }
}
