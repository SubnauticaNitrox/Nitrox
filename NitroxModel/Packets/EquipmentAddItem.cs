using NitroxModel.DataStructures.GameLogic;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EquipmentAddItem : Packet
    {
        public ItemEquipment ItemData { get; }
        public string Slot { get; }

        public EquipmentAddItem(ItemEquipment itemData, string slot)
        {
            ItemData = itemData;
            Slot = slot;
        }

        public override string ToString()
        {
            return "[EquipmentAddItem ItemData: " + ItemData + "]";
        }
    }
}
