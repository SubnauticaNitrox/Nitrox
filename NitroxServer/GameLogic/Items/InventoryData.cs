﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Items
{
    [DataContract]
    public class InventoryData
    {
        [DataMember(Order = 1)]
        public List<ItemData> InventoryItems = new List<ItemData>();

        [DataMember(Order = 2)]
        public List<ItemData> StorageSlotItems = new List<ItemData>();

        [DataMember(Order = 3)]
        public List<EquippedItemData> Modules { get; set; } = new List<EquippedItemData>();

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
}
