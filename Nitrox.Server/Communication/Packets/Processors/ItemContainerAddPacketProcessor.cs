using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Items;

namespace Nitrox.Server.Communication.Packets.Processors
{
    class ItemContainerAddPacketProcessor : AuthenticatedPacketProcessor<ItemContainerAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly InventoryManager inventoryManager;

        public ItemContainerAddPacketProcessor(PlayerManager playerManager, InventoryManager inventoryManager)
        {
            this.playerManager = playerManager;
            this.inventoryManager = inventoryManager;
        }

        public override void Process(ItemContainerAdd packet, Player player)
        {
            inventoryManager.ItemAdded(packet.ItemData);
            if (packet.ItemData.ContainerId != player.GameObjectId)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}
