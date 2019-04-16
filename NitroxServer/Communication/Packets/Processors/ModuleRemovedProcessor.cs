using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Players;

namespace NitroxServer.Communication.Packets.Processors
{
    class ModuleRemovedProcessor : AuthenticatedPacketProcessor<ModuleRemoved>
    {
        private readonly PlayerManager playerManager;
        private readonly PlayerData playerData;

        public ModuleRemovedProcessor(PlayerManager playerManager, PlayerData playerData)
        {
            this.playerManager = playerManager;
            this.playerData = playerData;
        }

        public override void Process(ModuleRemoved packet, Player player)
        {
            playerData.RemoveModule(packet.ItemId);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
