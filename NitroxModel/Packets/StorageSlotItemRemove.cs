using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class StorageSlotItemRemove : Packet
    {
        [Index(0)]
        public virtual NitroxId OwnerId { get; protected set; }

        public StorageSlotItemRemove() { }

        public StorageSlotItemRemove(NitroxId ownerId)
        {
            OwnerId = ownerId;
        }
    }
}
