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

        public ModuleRemoved(NitroxId ownerId, string slot, NitroxId itemId)
        {
            OwnerId = ownerId;
            Slot = slot;
            ItemId = itemId;
        }

        public override string ToString()
        {
            return $"[ModuleRemoved - OwnerId: {OwnerId}, Slot: {Slot}, ItemId: {ItemId}]";
        }
    }
}
