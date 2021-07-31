using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ModuleRemoved : Packet
    {
        public NitroxId OwnerId { get; }
        public string Slot { get; }
        public NitroxId ItemId { get; }
        public bool PlayerModule { get; }

        public ModuleRemoved(NitroxId ownerId, string slot, NitroxId itemId, bool playerModule)
        {
            OwnerId = ownerId;
            Slot = slot;
            ItemId = itemId;
            PlayerModule = playerModule;
        }
    }
}
