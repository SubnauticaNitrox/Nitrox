using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class ModuleRemoved : Packet
    {
        [Index(0)]
        public virtual NitroxId OwnerId { get; protected set; }
        [Index(1)]
        public virtual string Slot { get; protected set; }
        [Index(2)]
        public virtual NitroxId ItemId { get; protected set; }
        [Index(3)]
        public virtual bool PlayerModule { get; protected set; }

        private ModuleRemoved() { }

        public ModuleRemoved(NitroxId ownerId, string slot, NitroxId itemId, bool playerModule)
        {
            OwnerId = ownerId;
            Slot = slot;
            ItemId = itemId;
            PlayerModule = playerModule;
        }
    }
}
