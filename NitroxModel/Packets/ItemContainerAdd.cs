using NitroxModel.DataStructures.GameLogic;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemContainerAdd : Packet
    {
        public ItemData ItemData { get; }

        public ItemContainerAdd(ItemData itemData)
        {
            ItemData = itemData;
        }

        public override string ToString()
        {
            return "[ItemContainerAdd ItemData: " + ItemData + "]";
        }
    }
}
