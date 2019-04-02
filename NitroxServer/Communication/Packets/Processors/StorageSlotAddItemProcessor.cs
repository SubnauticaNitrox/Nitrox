using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.Communication.Packets.Processors
{
    class StorageSlotAddItemProcessor : AuthenticatedPacketProcessor<StorageSlotItemAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly InventoryData inventoryData;

        public StorageSlotAddItemProcessor(PlayerManager playerManager, InventoryData inventoryData)
        {
            this.playerManager = playerManager;
            this.inventoryData = inventoryData;
        }

        public override void Process(StorageSlotItemAdd packet, Player player)
        {
            inventoryData.StorageItemAdded(packet.ItemData);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
