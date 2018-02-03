using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemContainerRemove : PlayerActionPacket
    {
        public string OwnerGuid { get; }
        public string ItemGuid { get; }

        public ItemContainerRemove(string ownerGuid, string itemGuid, Vector3 ownerPositon) : base(ownerPositon)
        {
            OwnerGuid = ownerGuid;
            ItemGuid = itemGuid;
        }

        public override string ToString()
        {
            return "[ItemContainerRemove OwnerGuid: " + OwnerGuid + " ItemGuid: " + ItemGuid + "]";
        }
    }
}
