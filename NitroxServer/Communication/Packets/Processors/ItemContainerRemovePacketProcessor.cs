using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.Communication.Packets.Processors
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
