using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EquipmentRemoveItem : PlayerActionPacket
    {
        public String OwnerGuid { get; }
        public String Slot { get; }
        public String ItemGuid { get; }

        public EquipmentRemoveItem(String playerId, String ownerGuid, String slot, String itemGuid, Vector3 ownerPosition) : base(playerId, ownerPosition)
        {
            OwnerGuid = ownerGuid;
            Slot = slot;
            ItemGuid = itemGuid;
        }

        public override string ToString()
        {
            return "[EquipmentRemoveItem - playerId: " + PlayerId + " OwnerGuid: " + OwnerGuid + " Slot: " + Slot + " ItemGuid: " + ItemGuid + "]";
        }
    }
}
