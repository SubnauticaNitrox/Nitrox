using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemContainerAdd : Packet
    {
        public string OwnerGuid { get; }
        public byte[] ItemData { get; }

        public ItemContainerAdd(string ownerGuid, byte[] itemData)
        {
            OwnerGuid = ownerGuid;
            ItemData = itemData;
        }

        public override string ToString()
        {
            return "[ItemContainerAdd OwnerGuid: " + OwnerGuid + " Total Bytes: " + ItemData.Length + "]";
        }
    }
}
