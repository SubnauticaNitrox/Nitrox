using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;

namespace Nitrox.Server.Communication.Packets.Processors
{
    class FireDousedProcessor : AuthenticatedPacketProcessor<FireDoused>
    {
        private readonly PlayerManager playerManager;

        public FireDousedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(FireDoused packet, Player simulatingPlayer)
        {
            playerManager.SendPacketToOtherPlayers(packet, simulatingPlayer);
        }
    }
}
