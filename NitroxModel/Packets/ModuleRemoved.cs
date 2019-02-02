using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ModuleRemoved : Packet
    {
        public string OwnerGuid { get; }
        public string Slot { get; }
        public string ItemGuid { get; }

        public ModuleRemoved(string ownerGuid, string slot, string itemGuid)
        {
            OwnerGuid = ownerGuid;
            Slot = slot;
            ItemGuid = itemGuid;
        }

        public override string ToString()
        {
            return "[ModuleRemoved OwnerGuid: " + OwnerGuid + " Slot: " + Slot + " ItemGuid: " + ItemGuid + "]";
        }
    }
}
