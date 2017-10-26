using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemContainerAdd : PlayerActionPacket
    {
        public string OwnerGuid { get; }
        public byte[] ItemData { get; }

        public ItemContainerAdd(string playerId, string ownerGuid, byte[] itemData, Vector3 ownerPositon) : base(playerId, ownerPositon)
        {
            OwnerGuid = ownerGuid;
            ItemData = itemData;
        }

        public override string ToString()
        {
            return "[ItemContainerAdd - playerId: " + PlayerId + " OwnerGuid: " + OwnerGuid + " Total Bytes: " + ItemData.Length + "]";
        }
    }
}
