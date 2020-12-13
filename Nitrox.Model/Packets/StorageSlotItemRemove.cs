using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
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
            return "[StorageSlotItemRemove ownerId: " + OwnerId + "]";
        }
    }
}
