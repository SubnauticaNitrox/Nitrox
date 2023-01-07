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

        [Obsolete("To be migrated to the new entity system.")]
        private readonly ThreadSafeDictionary<NitroxId, EquippedItemData> modulesById;

        public InventoryManager(List<ItemData> storageSlotItems, List<EquippedItemData> modules)
        {
            storageSlotItemsByContainerId = new ThreadSafeDictionary<NitroxId, ItemData>(storageSlotItems.ToDictionary(item => item.ContainerId), false);
            modulesById = new ThreadSafeDictionary<NitroxId, EquippedItemData>(modules.ToDictionary(module => module.ItemId), false);
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
