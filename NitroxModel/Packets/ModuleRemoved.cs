using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets.Core;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ModuleRemoved : Packet, IVolatilePacket
    {
        public NitroxId OwnerId { get; }
        public string Slot { get; }
        public NitroxId ItemId { get; }

        public ModuleRemoved(NitroxId ownerId, string slot, NitroxId itemId)
        {
            OwnerId = ownerId;
            Slot = slot;
            ItemId = itemId;
        }

        public override string ToString()
        {
            return "[ModuleRemoved ownerId: " + OwnerId + " Slot: " + Slot + " itemId: " + ItemId + "]";
        }
    }
}
