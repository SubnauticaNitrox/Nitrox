using System;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class StorageSlotItemAdd : Packet
    {
        public ItemData ItemData { get; }

        public StorageSlotItemAdd(ItemData itemData)
        {
            ItemData = itemData;
        }

        public override string ToString()
        {
            return "[StorageSlotItemAdd ItemData: " + ItemData + "]";
        }
    }
}
