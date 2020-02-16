using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.Serialization.World;
using NitroxModel.DataStructures.Util;

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
            Log.Info($"{packet}");
            world.PlayerData.UpdatePlayerSubRootId(player.Name, packet.SubRootId.OrElse(null));
            player.SubRootId = packet.SubRootId;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
