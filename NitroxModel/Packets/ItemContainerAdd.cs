using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemContainerAdd : PlayerActionPacket
    {
        public String OwnerGuid { get; private set; }
        public byte[] ItemData { get; private set; }

        public ItemContainerAdd(String playerId, String ownerGuid, byte[] itemData, Vector3 ownerPositon) : base(playerId, ownerPositon)
        {
            this.OwnerGuid = ownerGuid;
            this.ItemData = itemData;
        }

        public override string ToString()
        {
            return "[ItemContainerAdd - playerId: " + PlayerId + " OwnerGuid: " + OwnerGuid + " Total Bytes: " + ItemData.Length + "]";
        }
    }
}
