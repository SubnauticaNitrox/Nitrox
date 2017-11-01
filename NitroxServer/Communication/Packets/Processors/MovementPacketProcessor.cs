using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class MovementPacketProcessor : AuthenticatedPacketProcessor<Movement>
    {
        private readonly PlayerManager playerManager;

        public MovementPacketProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(Movement packet, Player player)
        {
            player.Position = packet.Position;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
