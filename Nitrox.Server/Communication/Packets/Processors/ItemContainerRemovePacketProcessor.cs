using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Items;

namespace Nitrox.Server.Communication.Packets.Processors
{
    class ItemContainerRemovePacketProcessor : AuthenticatedPacketProcessor<ItemContainerRemove>
    {
        private readonly PlayerManager playerManager;
        private readonly InventoryManager inventoryManager;

        public ItemContainerRemovePacketProcessor(PlayerManager playerManager, InventoryManager inventoryManager)
        {
            this.playerManager = playerManager;
            this.inventoryManager = inventoryManager;
        }

        public override void Process(ItemContainerRemove packet, Player player)
        {
            inventoryManager.ItemRemoved(packet.ItemId);
            if (packet.OwnerId != player.GameObjectId)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}
