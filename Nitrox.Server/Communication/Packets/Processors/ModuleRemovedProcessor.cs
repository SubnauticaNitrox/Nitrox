using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;

namespace Nitrox.Server.Communication.Packets.Processors
{
    class ModuleRemovedProcessor : AuthenticatedPacketProcessor<ModuleRemoved>
    {
        private readonly PlayerManager playerManager;

        public ModuleRemovedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(ModuleRemoved packet, Player player)
        {
            player.RemoveModule(packet.ItemId);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
