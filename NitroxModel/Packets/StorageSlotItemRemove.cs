using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets.Core;

namespace NitroxModel.Packets
{
    [Serializable]
    public class StorageSlotItemRemove : Packet, IVolatilePacket
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
