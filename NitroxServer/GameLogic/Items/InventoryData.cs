using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using System.Linq;

namespace NitroxServer.GameLogic.Items
{
    [ProtoContract]
    public class InventoryData
    {
        [ProtoMember(1)]
        private List<ItemData> serializableInsertedInventoryItems = new List<ItemData>();

        [ProtoMember(2)]
        private List<ItemData> serializableStorageSlotItems = new List<ItemData>();
        
        [ProtoIgnore]
        private Dictionary<NitroxId, ItemData> insertedInventoryItemsById = new Dictionary<NitroxId, ItemData>();

        private Dictionary<NitroxId, ItemData> storageSlotItemsById = new Dictionary<NitroxId, ItemData>();

        public void ItemAdded(ItemData itemData)
        {
            lock(insertedInventoryItemsById)
            {
                insertedInventoryItemsById[itemData.ItemId] = itemData;
            }
        }

        public void ItemRemoved(NitroxId itemId)
        {
            lock (insertedInventoryItemsById)
            {
                insertedInventoryItemsById.Remove(itemId);
            }
        }
        
        public List<ItemData> GetAllItemsForInitialSync()
        {
            lock (insertedInventoryItemsById)
            {
                return new List<ItemData>(insertedInventoryItemsById.Values);
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

        public List<ItemData> GetAllStorageItemsForInitialSync()
        {
            lock (storageSlotItemsById)
            {
                return new List<ItemData>(storageSlotItemsById.Values);
            }
        }

        [ProtoBeforeSerialization]
        private void BeforeSerialization()
        {
            serializableInsertedInventoryItems = insertedInventoryItemsById.Values.ToList();
            serializableStorageSlotItems = storageSlotItemsById.Values.ToList();
        }

        [ProtoAfterDeserialization]
        private void AfterDeserialization()
        {
            foreach (ItemData item in serializableInsertedInventoryItems)
            {
                insertedInventoryItemsById.Add(item.ItemId, item);
            }
            foreach (ItemData item in serializableStorageSlotItems)
            {
                insertedInventoryItemsById.Add(item.ContainerId, item);
            }
            serializableStorageSlotItems = null;
            serializableInsertedInventoryItems = null;
        }
    }
}
