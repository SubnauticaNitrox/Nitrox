using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EquipmentAddItem : DeferrablePacket
    {
        public string OwnerGuid { get; }
        public string Slot { get; }
        public byte[] ItemBytes { get; }

        public EquipmentAddItem(string ownerGuid, string slot, byte[] itemBytes, Vector3 ownerPosition) : base(ownerPosition, ITEM_INTERACTION_CELL_LEVEL)
        {
            OwnerGuid = ownerGuid;
            Slot = slot;
            ItemBytes = itemBytes;
        }

        public override string ToString()
        {
            return "[EquipmentAddItem OwnerGuid: " + OwnerGuid + " Slot: " + Slot + " Total Item Bytes: " + ItemBytes.Length + "]";
        }
    }
}
