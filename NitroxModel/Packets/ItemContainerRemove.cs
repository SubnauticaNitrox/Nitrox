using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemContainerRemove : PlayerActionPacket
    {
        public string OwnerGuid { get; }
        public string ItemGuid { get; }

        public ItemContainerRemove(string playerId, string ownerGuid, string itemGuid, Vector3 ownerPositon) : base(playerId, ownerPositon)
        {
            OwnerGuid = ownerGuid;
            ItemGuid = itemGuid;
        }

        public override string ToString()
        {
            return "[ItemContainerRemove - playerId: " + PlayerId + " OwnerGuid: " + OwnerGuid + " ItemGuid: " + ItemGuid + "]";
        }
    }
}
