using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Items
{
    [DataContract]
    public class InventoryData
    {
        [DataMember(Order = 1)]
        public List<ItemData> StorageSlotItems = new List<ItemData>();

        public static InventoryData From(IEnumerable<ItemData> storageSlotItems)
        {
            return new InventoryData
            {
                StorageSlotItems = storageSlotItems.ToList()
            };
        }

        public override string ToString()
        {
            return $"[{nameof(StorageSlotItems)}: {StorageSlotItems.Count}]";
        }
    }
}
