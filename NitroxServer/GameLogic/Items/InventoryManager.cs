using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Items
{
    public class InventoryManager
    {
        [Obsolete("To be migrated to the new entity system.")]
        private readonly ThreadSafeDictionary<NitroxId, ItemData> storageSlotItemsByContainerId;

        public InventoryManager(List<ItemData> storageSlotItems)
        {
            storageSlotItemsByContainerId = new ThreadSafeDictionary<NitroxId, ItemData>(storageSlotItems.ToDictionary(item => item.ContainerId), false);
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
    }
}
