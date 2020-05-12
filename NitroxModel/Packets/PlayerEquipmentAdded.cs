using System;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerEquipmentAdded : Packet
    {
        public DataStructures.TechType TechType { get; }
        public EquippedItemData EquippedItem { get; }

        public PlayerEquipmentAdded(DataStructures.TechType techType, EquippedItemData equippedItem)
        {
            TechType = techType;
            EquippedItem = equippedItem;
        }
    }
}
