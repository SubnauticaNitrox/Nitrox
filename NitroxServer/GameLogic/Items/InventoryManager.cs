using NitroxModel.DataStructures.GameLogic;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using System.Linq;

namespace NitroxServer.GameLogic.Items
{
    public class InventoryManager
    {
        private Dictionary<NitroxId, ItemData> inventoryItemsById;
        private Dictionary<NitroxId, ItemData> storageSlotItemsById;

        public InventoryManager(List<ItemData> inventoryItems, List<ItemData> storageSlotItems)
        {
            inventoryItemsById = inventoryItems.ToDictionary(item => item.ItemId);
            storageSlotItemsById = storageSlotItems.ToDictionary(item => item.ItemId);
        }

        public void ItemAdded(ItemData itemData)
        {
            lock (inventoryItemsById)
            {
                inventoryItemsById[itemData.ItemId] = itemData;
            }
        }

        public void ItemRemoved(NitroxId itemId)
        {
            lock (inventoryItemsById)
            {
                inventoryItemsById.Remove(itemId);
            }
        }

        public List<ItemData> GetAllInventoryItems()
        {
            lock (inventoryItemsById)
            {
                return new List<ItemData>(inventoryItemsById.Values);
            }
        }
        
        public void StorageItemAdded(ItemData itemData)
        {
            lock (storageSlotItemsById)
            {
                storageSlotItemsById[itemData.ContainerId] = itemData;
            }
        }

        public bool StorageItemRemoved(NitroxId ownerId)
        {
            bool success = false;
            lock (storageSlotItemsById)
            {
                success = storageSlotItemsById.Remove(ownerId);
            }
            return success;
        }

        public List<ItemData> GetAllStorageSlotItems()
        {
            lock (storageSlotItemsById)
            {
                return new List<ItemData>(storageSlotItemsById.Values);
            }
        }
    }
}
