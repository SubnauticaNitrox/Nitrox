using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.Communication.Packets.Processors
{
    class StorageSlotRemoveItemProcessor : AuthenticatedPacketProcessor<StorageSlotItemRemove>
    {
        private readonly PlayerManager playerManager;
        private readonly InventoryData inventoryData;

        public StorageSlotRemoveItemProcessor(PlayerManager playerManager, InventoryData inventoryData)
        {
            this.playerManager = playerManager;
            this.inventoryData = inventoryData;
        }

        public override void Process(StorageSlotItemRemove packet, Player player)
        {
            // Only need to send to other players, if an synched item was really removed
            if (inventoryData.StorageItemRemoved(packet.OwnerGuid)) 
            {                
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}
