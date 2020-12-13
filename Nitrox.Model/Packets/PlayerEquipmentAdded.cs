using System;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class PlayerEquipmentAdded : Packet
    {
        public NitroxTechType TechType { get; }
        public EquippedItemData EquippedItem { get; }

        public PlayerEquipmentAdded(NitroxTechType techType, EquippedItemData equippedItem)
        {
            TechType = techType;
            EquippedItem = equippedItem;
        }
    }
}
