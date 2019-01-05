using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.Serialization.World;

namespace NitroxServer.Communication.Packets.Processors
{
    class SubRootChangedPacketProcessor : AuthenticatedPacketProcessor<SubRootChanged>
    {
        private readonly PlayerManager playerManager;
        private readonly World world;

        public SubRootChangedPacketProcessor(PlayerManager playerManager, World world)
        {
            this.playerManager = playerManager;
            this.world = world;
        }

        public override void Process(SubRootChanged packet, Player player)
        {
            Log.Info(packet);
            world.PlayerData.UpdatePlayerSubRootGuid(player.Name, packet.SubRootGuid.OrElse(null));
            player.SubRootGuid = packet.SubRootGuid;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
