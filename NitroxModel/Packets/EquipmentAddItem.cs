using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EquipmentAddItem : PlayerActionPacket
    {
        public String OwnerGuid { get; }
        public String Slot { get; }
        public byte[] ItemBytes { get; }

        public EquipmentAddItem(String playerId, String ownerGuid, String slot, byte[] itemBytes, Vector3 ownerPosition) : base(playerId, ownerPosition)
        {
            this.OwnerGuid = ownerGuid;
            this.Slot = slot;
            this.ItemBytes = itemBytes;
        }

        public override string ToString()
        {
            return "[EquipmentAddItem - playerId: " + PlayerId + " OwnerGuid: " + OwnerGuid + " Slot: " + Slot + " Total Item Bytes: " + ItemBytes.Length + "]";
        }
    }
}
