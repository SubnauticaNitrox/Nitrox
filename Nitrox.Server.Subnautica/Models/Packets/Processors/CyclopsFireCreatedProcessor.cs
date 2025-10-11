using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    class CyclopsFireCreatedProcessor : AuthenticatedPacketProcessor<CyclopsFireCreated>
    {
        private readonly PlayerManager playerManager;

        public CyclopsFireCreatedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(CyclopsFireCreated packet, Player simulatingPlayer)
        {
            playerManager.SendPacketToOtherPlayers(packet, simulatingPlayer);
        }
    }
}
