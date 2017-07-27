using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemContainerRemove : PlayerActionPacket
    {
        public String OwnerGuid { get; private set; }
        public String ItemGuid { get; private set; }

        public ItemContainerRemove(String playerId, String ownerGuid, String itemGuid, Vector3 ownerPositon) : base(playerId, ownerPositon)
        {
            this.OwnerGuid = ownerGuid;
            this.ItemGuid = itemGuid;
        }

        public override string ToString()
        {
            return "[ItemContainerRemove - playerId: " + PlayerId + " OwnerGuid: " + OwnerGuid + " ItemGuid: " + ItemGuid + "]";
        }
    }
}
