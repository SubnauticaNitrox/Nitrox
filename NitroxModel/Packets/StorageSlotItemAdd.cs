using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
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
