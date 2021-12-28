using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class StorageSlotItemAdd : Packet
    {
        [Index(0)]
        public virtual ItemData ItemData { get; protected set; }

        public StorageSlotItemAdd() { }

        public StorageSlotItemAdd(ItemData itemData)
        {
            ItemData = itemData;
        }
    }
}
