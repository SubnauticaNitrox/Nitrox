﻿using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerEquipmentAdded : Packet
    {
        public TechType TechType { get; }
        public EquippedItemData EquippedItem { get; }

        public PlayerEquipmentAdded(TechType techType, EquippedItemData equippedItem)
        {
            TechType = techType;
            EquippedItem = equippedItem;
        }
    }
}
