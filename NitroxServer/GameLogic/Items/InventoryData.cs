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
        public Dictionary<NitroxId, ItemData> SerializableInsertedInventoryItemsById
        {
            get
            {
                lock (insertedInventoryItemsById)
                {
                    return new Dictionary<NitroxId, ItemData>(insertedInventoryItemsById);
                }
            }
            set { insertedInventoryItemsById = value; }
        }

        [ProtoMember(2)]
        public Dictionary<NitroxId, ItemData> SerializableStorageSlotItemsById
        {
            get
            {
                lock (storageSlotItemsById)
                {
                    return new Dictionary<NitroxId, ItemData>(storageSlotItemsById);
                }
            }
            set { storageSlotItemsById = value; }
        }

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
    }
}
