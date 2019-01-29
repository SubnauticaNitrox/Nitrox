using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Players;

namespace NitroxServer.Communication.Packets.Processors
{
    class ModuleAddedProcessor : AuthenticatedPacketProcessor<ModuleAdded>
    {
        private readonly PlayerManager playerManager;
        private readonly PlayerData playerData;

        public ModuleAddedProcessor(PlayerManager playerManager, PlayerData playerData)
        {
            this.playerManager = playerManager;
            this.playerData = playerData;
        }

        public override void Process(ModuleAdded packet, Player player)
        {
            playerData.AddModule(packet.EquippedItemData);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
