using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EquipmentAddItem : PlayerActionPacket
    {
        public string OwnerGuid { get; }
        public string Slot { get; }
        public byte[] ItemBytes { get; }

        public EquipmentAddItem(string playerId, string ownerGuid, string slot, byte[] itemBytes, Vector3 ownerPosition) : base(playerId, ownerPosition)
        {
            OwnerGuid = ownerGuid;
            Slot = slot;
            ItemBytes = itemBytes;
        }

        public override string ToString()
        {
            return "[EquipmentAddItem - playerId: " + PlayerId + " OwnerGuid: " + OwnerGuid + " Slot: " + Slot + " Total Item Bytes: " + ItemBytes.Length + "]";
        }
    }
}
