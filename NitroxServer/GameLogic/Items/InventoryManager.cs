using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;

namespace NitroxServer.GameLogic.Items
{
    public class InventoryManager
    {
        private readonly ThreadSafeDictionary<NitroxId, ItemData> inventoryItemsById;
        private readonly ThreadSafeDictionary<NitroxId, ItemData> storageSlotItemsById;

        public InventoryManager(List<ItemData> inventoryItems, List<ItemData> storageSlotItems)
        {
            inventoryItemsById = new ThreadSafeDictionary<NitroxId, ItemData>(inventoryItems.ToDictionary(item => item.ItemId), false);
            storageSlotItemsById = new ThreadSafeDictionary<NitroxId, ItemData>(storageSlotItems.ToDictionary(item => item.ItemId), false);
        }

        public void ItemAdded(ItemData itemData)
        {
            inventoryItemsById[itemData.ItemId] = itemData;
            Log.Debug($"Received inventory item '{itemData.ItemId}' to container '{itemData.ContainerId}'. Total items: {inventoryItemsById.Count}");
        }

        public void ItemRemoved(NitroxId itemId)
        {
            inventoryItemsById.Remove(itemId);
        }

        public ICollection<ItemData> GetAllInventoryItems()
        {
            return inventoryItemsById.Values;
        }

        public void StorageItemAdded(ItemData itemData)
        {
            storageSlotItemsById[itemData.ContainerId] = itemData;
        }

        public bool StorageItemRemoved(NitroxId ownerId)
        {
            return storageSlotItemsById.Remove(ownerId);
        }

        public ICollection<ItemData> GetAllStorageSlotItems()
        {
            return storageSlotItemsById.Values;
        }
    }
}
