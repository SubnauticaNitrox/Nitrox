using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Items
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class InventoryData
    {
        [JsonProperty, ProtoMember(1)]
        public List<ItemData> InventoryItems = new List<ItemData>();

        [JsonProperty, ProtoMember(2)]
        public List<ItemData> StorageSlotItems = new List<ItemData>();

        private InventoryData()
        {
        }

        public static InventoryData From(IEnumerable<ItemData> inventoryItems, IEnumerable<ItemData> storageSlotItems)
        {
            return new InventoryData
            {
                InventoryItems = inventoryItems.ToList(),
                StorageSlotItems = storageSlotItems.ToList()
            };
        }

        public override string ToString()
        {
            return $"[{nameof(InventoryData)} - {nameof(InventoryItems)}: {InventoryItems.Count}, {nameof(StorageSlotItems)}: {StorageSlotItems.Count}]";
        }
    }
}
