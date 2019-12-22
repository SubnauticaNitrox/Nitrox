using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.Communication.Packets.Processors
{
    class ItemContainerAddPacketProcessor : AuthenticatedPacketProcessor<ItemContainerAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly InventoryData inventoryData;

        public ItemContainerAddPacketProcessor(PlayerManager playerManager, InventoryData inventoryData)
        {
            this.playerManager = playerManager;
            this.inventoryData = inventoryData;
        }

        public override void Process(ItemContainerAdd packet, Player player)
        {
            inventoryData.ItemAdded(packet.ItemData);
            if (packet.ItemData.ContainerId != player.Id)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}
