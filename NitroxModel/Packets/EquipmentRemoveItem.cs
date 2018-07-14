﻿using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EquipmentRemoveItem : Packet
    {
        public string OwnerGuid { get; }
        public string Slot { get; }
        public string ItemGuid { get; }
        public bool IsPlayerEquipment { get; }

        public EquipmentRemoveItem(string ownerGuid, string slot, string itemGuid, bool isPlayerEquipment)
        {
            IsPlayerEquipment = isPlayerEquipment;
            OwnerGuid = ownerGuid;
            Slot = slot;
            ItemGuid = itemGuid;
        }

        public override string ToString()
        {
            return "[EquipmentRemoveItem OwnerGuid: " + OwnerGuid + " Slot: " + Slot + " ItemGuid: " + ItemGuid + " IsPlayerEquipment: " + IsPlayerEquipment + " ]";
        }
    }
}
