using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.Communication.Packets.Processors
{
    class ModuleAddedProcessor : AuthenticatedPacketProcessor<ModuleAdded>
    {
        private readonly PlayerManager playerManager;
        private readonly InventoryManager inventoryManager;

        public ModuleAddedProcessor(PlayerManager playerManager, InventoryManager inventoryManager)
        {
            this.playerManager = playerManager;
            this.inventoryManager = inventoryManager;
        }

        public override void Process(ModuleAdded packet, Player player)
        {
            if (packet.PlayerModule)
            {
                player.AddModule(packet.EquippedItemData);
            }
            else
            {
                inventoryManager.ModuleAdded(packet.EquippedItemData);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
