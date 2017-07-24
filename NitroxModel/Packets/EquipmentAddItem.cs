using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EquipmentAddItem : PlayerActionPacket
    {
        public String OwnerGuid { get; private set; }
        public String Slot { get; private set; }
        public byte[] ItemBytes { get; private set; }

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
