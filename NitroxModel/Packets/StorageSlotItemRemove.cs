using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class StorageSlotItemRemove : Packet
    {
        public NitroxId OwnerId { get; }

        public StorageSlotItemRemove(NitroxId ownerId)
        {
            OwnerId = ownerId;
        }

        public override string ToString()
        {
            return $"[StorageSlotItemRemove - OwnerId: {OwnerId}]";
        }
    }
}
