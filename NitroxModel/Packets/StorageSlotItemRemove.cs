using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class StorageSlotItemRemove : Packet
    {
        public string OwnerGuid { get; }

        public StorageSlotItemRemove(string ownerGuid)
        {
            OwnerGuid = ownerGuid;
        }

        public override string ToString()
        {
            return "[StorageSlotItemRemove OwnerGuid: " + OwnerGuid + "]";
        }
    }
}
