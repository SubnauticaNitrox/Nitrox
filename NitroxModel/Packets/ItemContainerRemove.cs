using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemContainerRemove : DeferrablePacket
    {
        public string OwnerGuid { get; }
        public string ItemGuid { get; }

        public ItemContainerRemove(string ownerGuid, string itemGuid, Vector3 ownerPositon) : base(ownerPositon, ITEM_INTERACTION_CELL_LEVEL)
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
