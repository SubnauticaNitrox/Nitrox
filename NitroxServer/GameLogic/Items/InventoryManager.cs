using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Items
{
    public class InventoryManager
    {
        private readonly ThreadSafeDictionary<NitroxId, ItemData> inventoryItemsById;
        private readonly ThreadSafeDictionary<NitroxId, ItemData> storageSlotItemsByContainerId;
        private readonly ThreadSafeDictionary<NitroxId, EquippedItemData> modulesById;

        public InventoryManager(List<ItemData> inventoryItems, List<ItemData> storageSlotItems, List<EquippedItemData> modules)
        {
            inventoryItemsById = new ThreadSafeDictionary<NitroxId, ItemData>(inventoryItems.ToDictionary(item => item.ItemId), false);
            storageSlotItemsByContainerId = new ThreadSafeDictionary<NitroxId, ItemData>(storageSlotItems.ToDictionary(item => item.ContainerId), false);
            modulesById = new ThreadSafeDictionary<NitroxId, EquippedItemData>(modules.ToDictionary(module => module.ItemId), false);
        }

        public void InventoryItemAdded(ItemData itemData)
        {
            inventoryItemsById[itemData.ItemId] = itemData;
            Log.Debug($"Received inventory item {itemData.ItemId} to container {itemData.ContainerId}. Total items: {inventoryItemsById.Count}");
        }

        public void InventoryItemRemoved(NitroxId itemId)
        {
            inventoryItemsById.Remove(itemId);
        }

        public ICollection<ItemData> GetAllInventoryItems()
        {
            return inventoryItemsById.Values;
        }

        public void StorageItemAdded(ItemData itemData)
        {
            storageSlotItemsByContainerId[itemData.ContainerId] = itemData;
        }

        public bool StorageItemRemoved(NitroxId ownerId)
        {
            return storageSlotItemsByContainerId.Remove(ownerId);
        }

        public ICollection<ItemData> GetAllStorageSlotItems()
        {
            return storageSlotItemsByContainerId.Values;
        }

        public void ModuleAdded(EquippedItemData itemData)
        {
            modulesById[itemData.ItemId] = itemData;
        }

        public bool ModuleRemoved(NitroxId ownerId)
        {
            return modulesById.Remove(ownerId);
        }

        public ICollection<EquippedItemData> GetAllModules()
        {
            return modulesById.Values;
        }
    }
}
