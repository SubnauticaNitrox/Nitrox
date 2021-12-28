using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class ItemContainerAdd : Packet
    {
        [Index(0)]
        public virtual ItemData ItemData { get; protected set; }

        public ItemContainerAdd() { }

        public ItemContainerAdd(ItemData itemData)
        {
            ItemData = itemData;
        }
    }
}
