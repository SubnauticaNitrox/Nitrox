using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EquipmentRemoveItem : RangedPacket
    {
        public string OwnerGuid { get; }
        public string Slot { get; }
        public string ItemGuid { get; }

        public EquipmentRemoveItem(string ownerGuid, string slot, string itemGuid, Vector3 ownerPosition) : base(ownerPosition, ITEM_INTERACTION_CELL_LEVEL)
        {
            OwnerGuid = ownerGuid;
            Slot = slot;
            ItemGuid = itemGuid;
        }

        public override string ToString()
        {
            return "[EquipmentRemoveItem OwnerGuid: " + OwnerGuid + " Slot: " + Slot + " ItemGuid: " + ItemGuid + "]";
        }
    }
}
