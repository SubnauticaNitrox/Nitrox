using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemContainerRemove : Packet
    {
        public NitroxId OwnerId { get; }
        public NitroxId ItemId { get; }

        public ItemContainerRemove(NitroxId ownerId, NitroxId itemId)
        {
            OwnerId = ownerId;
            ItemId = itemId;
        }
    }
}
