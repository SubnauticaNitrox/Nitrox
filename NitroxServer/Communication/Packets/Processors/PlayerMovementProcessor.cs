using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class PlayerMovementProcessor : AuthenticatedPacketProcessor<PlayerMovement>
    {
        private readonly PlayerManager playerManager;

        public PlayerMovementProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(PlayerMovement packet, Player player)
        {
            player.Position = packet.Position;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
