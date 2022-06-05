using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.Serialization.SaveData;

[JsonObject(MemberSerialization.OptIn)]
public class InventoryData
{
    [JsonProperty]
    public List<ItemData> InventoryItems = new();

    [JsonProperty]
    public List<ItemData> StorageSlotItems = new();

    [JsonProperty]
    public List<EquippedItemData> Modules { get; set; } = new();

    public static InventoryData From(IEnumerable<ItemData> inventoryItems, IEnumerable<ItemData> storageSlotItems, IEnumerable<EquippedItemData> modules)
    {
        return new InventoryData
        {
            InventoryItems = inventoryItems.ToList(),
            StorageSlotItems = storageSlotItems.ToList(),
            Modules = modules.ToList()
        };
    }

    public override string ToString()
    {
        return $"[{nameof(InventoryData)} - {nameof(InventoryItems)}: {InventoryItems.Count}, {nameof(StorageSlotItems)}: {StorageSlotItems.Count}, {nameof(Modules)}: {Modules.Count}]";
    }
}
