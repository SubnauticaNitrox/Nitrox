using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
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
