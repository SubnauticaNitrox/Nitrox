using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemContainerRemove : PlayerActionPacket
    {
        public String OwnerGuid { get; }
        public String ItemGuid { get; }

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
