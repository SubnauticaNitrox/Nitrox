using NitroxModel.DataStructures.GameLogic;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EquipmentAddItem : Packet
    {
        public EquippedItemData EquippedItemData { get; }
        public bool IsPlayerEquipment { get; }

        public EquipmentAddItem(EquippedItemData equippedItemData, bool isPlayerEquipment)
        {
            IsPlayerEquipment = isPlayerEquipment;
            EquippedItemData = equippedItemData;
        }

        public override string ToString()
        {
            return "[EquipmentAddItem EquippedItemData: " + EquippedItemData + " IsPlayerEquipment: " + IsPlayerEquipment + " ]";
        }
    }
}
