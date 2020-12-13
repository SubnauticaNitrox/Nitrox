using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Items;

namespace Nitrox.Server.Communication.Packets.Processors
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
