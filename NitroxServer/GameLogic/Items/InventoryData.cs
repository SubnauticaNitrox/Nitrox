using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;
using NitroxModel.DataStructures;

namespace NitroxServer.GameLogic.Items
{
    [ProtoContract]
    public class InventoryData
    {
        [ProtoMember(1)]
        public Dictionary<NitroxId, ItemData> SerializableInsertedInventoryItemsById
        {
            get
            {
                lock (insertedInventoryItemsById)
                {
                    serializableInsertedInventoryItemsById = new Dictionary<NitroxId, ItemData>(insertedInventoryItemsById);
                    return serializableInsertedInventoryItemsById;
                }
            }
            set { insertedInventoryItemsById = value; }
        }

        private Dictionary<NitroxId, ItemData> serializableInsertedInventoryItemsById = new Dictionary<NitroxId, ItemData>();

        [ProtoMember(2)]
        public Dictionary<NitroxId, ItemData> SerializableStorageSlotItemsById
        {
            get
            {
                lock (storageSlotItemsById)
                {
                    serializableStorageSlotItemsById = new Dictionary<NitroxId, ItemData>(storageSlotItemsById);
                    return serializableStorageSlotItemsById;
                }
            }
            set { storageSlotItemsById = value; }
        }

        private Dictionary<NitroxId, ItemData> serializableStorageSlotItemsById = new Dictionary<NitroxId, ItemData>();

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

        [ProtoAfterDeserialization]
        private void AfterDeserialization()
        {
            insertedInventoryItemsById = serializableInsertedInventoryItemsById;
            storageSlotItemsById = serializableStorageSlotItemsById;
        }
    }
}
