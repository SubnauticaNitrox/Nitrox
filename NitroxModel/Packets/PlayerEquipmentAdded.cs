using System;
using NitroxModel.DataStructures.GameLogic;
using DTO = NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerEquipmentAdded : Packet
    {
        public PlayerEquipmentAdded(DTO.TechType techType, EquippedItemData equippedItem)
        {
            TechType = techType;
            EquippedItem = equippedItem;
        }

        public DTO.TechType TechType { get; }
        public EquippedItemData EquippedItem { get; }
    }
}
