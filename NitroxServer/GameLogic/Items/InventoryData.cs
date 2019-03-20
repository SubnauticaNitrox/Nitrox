using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Items
{
    [ProtoContract]
    public class InventoryData
    {
        [ProtoMember(1)]
        public Dictionary<string, ItemData> SerializableInsertedInventoryItemsByGuid
        {
            get
            {
                lock (insertedInventoryItemsByGuid)
                {
                    return new Dictionary<string, ItemData>(insertedInventoryItemsByGuid);
                }
            }
            set { insertedInventoryItemsByGuid = value; }
        }
        
        [ProtoMember(2)]
        public Dictionary<string, ItemData> SerializableStorageSlotItemsByGuid
        {
            get
            {
                lock (storageSlotItemsByGuid)
                {
                    return new Dictionary<string, ItemData>(storageSlotItemsByGuid);
                }
            }
            set { storageSlotItemsByGuid = value; }
        }
        
        [ProtoIgnore]
        private Dictionary<string, ItemData> insertedInventoryItemsByGuid = new Dictionary<string, ItemData>();

        private Dictionary<string, ItemData> storageSlotItemsByGuid = new Dictionary<string, ItemData>();

        public void ItemAdded(ItemData itemData)
        {
            lock(insertedInventoryItemsByGuid)
            {
                insertedInventoryItemsByGuid[itemData.Guid] = itemData;
            }
        }

        public void ItemRemoved(string itemGuid)
        {
            lock (insertedInventoryItemsByGuid)
            {
                insertedInventoryItemsByGuid.Remove(itemGuid);
            }
        }
        
        public List<ItemData> GetAllItemsForInitialSync()
        {
            lock (insertedInventoryItemsByGuid)
            {
                return new List<ItemData>(insertedInventoryItemsByGuid.Values);
            }
        }

        
        public void StorageItemAdded(ItemData itemData)
        {
            lock (storageSlotItemsByGuid)
            {
                storageSlotItemsByGuid[itemData.ContainerGuid] = itemData;
            }
        }

        public bool StorageItemRemoved(string ownerGuid)
        {
            bool success = false;
            lock (storageSlotItemsByGuid)
            {
                success = storageSlotItemsByGuid.Remove(ownerGuid);
            }
            return success;
        }

        public List<ItemData> GetAllStorageItemsForInitialSync()
        {
            lock (storageSlotItemsByGuid)
            {
                return new List<ItemData>(storageSlotItemsByGuid.Values);
            }
        }        
    }
}
