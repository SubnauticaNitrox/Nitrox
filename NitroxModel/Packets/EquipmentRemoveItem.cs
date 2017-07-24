using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EquipmentRemoveItem : PlayerActionPacket
    {
        public String OwnerGuid { get; private set; }
        public String Slot { get; private set; }
        public String ItemGuid { get; private set; }

        public EquipmentRemoveItem(String playerId, String ownerGuid, String slot, String itemGuid, Vector3 ownerPosition) : base(playerId, ownerPosition)
        {
            this.OwnerGuid = ownerGuid;
            this.Slot = slot;
            this.ItemGuid = itemGuid;
        }

        public override string ToString()
        {
            return "[EquipmentRemoveItem - playerId: " + PlayerId + " OwnerGuid: " + OwnerGuid + " Slot: " + Slot + " ItemGuid: " + ItemGuid + "]";
        }
    }
}
