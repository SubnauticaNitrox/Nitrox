using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.Communication.Packets.Processors
{
    class StorageSlotRemoveItemProcessor : AuthenticatedPacketProcessor<StorageSlotItemRemove>
    {
        private readonly PlayerManager playerManager;
        private readonly InventoryManager inventoryManager;

        public StorageSlotRemoveItemProcessor(PlayerManager playerManager, InventoryManager inventoryManager)
        {
            this.playerManager = playerManager;
            this.inventoryManager = inventoryManager;
        }

        public override void Process(StorageSlotItemRemove packet, Player player)
        {
            // Only need to send to other players, if an synced item was really removed
            if (inventoryManager.StorageItemRemoved(packet.OwnerId)) 
            {                
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}
