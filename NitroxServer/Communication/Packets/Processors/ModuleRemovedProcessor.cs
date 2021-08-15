using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.Communication.Packets.Processors
{
    class ModuleRemovedProcessor : AuthenticatedPacketProcessor<ModuleRemoved>
    {
        private readonly PlayerManager playerManager;
        private readonly InventoryManager inventoryManager;

        public ModuleRemovedProcessor(PlayerManager playerManager, InventoryManager inventoryManager)
        {
            this.playerManager = playerManager;
            this.inventoryManager = inventoryManager;
        }

        public override void Process(ModuleRemoved packet, Player player)
        {
            if (packet.PlayerModule)
            {
                player.RemoveModule(packet.ItemId);
            }
            else
            {
                inventoryManager.ModuleRemoved(packet.ItemId);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
