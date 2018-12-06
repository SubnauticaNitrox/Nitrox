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

        [ProtoIgnore]
        private Dictionary<string, ItemData> insertedInventoryItemsByGuid = new Dictionary<string, ItemData>();
        
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
    }
}
