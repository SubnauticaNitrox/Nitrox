using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.Communication.Packets.Processors
{
    public class ItemContainerAddPacketProcessor : AuthenticatedPacketProcessor<ItemContainerAdd>
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
            inventoryManager.InventoryItemAdded(packet.ItemData);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
