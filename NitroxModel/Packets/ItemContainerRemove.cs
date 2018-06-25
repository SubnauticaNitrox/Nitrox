using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemContainerRemove : Packet
    {
        public string OwnerGuid { get; }
        public string ItemGuid { get; }

        public ItemContainerRemove(string ownerGuid, string itemGuid)
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
