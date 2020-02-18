using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Items
{
    [ProtoContract]
    public class InventoryData
    {
        [ProtoMember(1)]
        public List<ItemData> InventoryItems;

        [ProtoMember(2)]
        public List<ItemData> StorageSlotItems;

        public static InventoryData From(List<ItemData> inventoryItems, List<ItemData> storageSlotItems)
        {
            InventoryData inventoryData = new InventoryData();
            inventoryData.InventoryItems = inventoryItems;
            inventoryData.StorageSlotItems = storageSlotItems;

            return inventoryData;
        }
    }
}
