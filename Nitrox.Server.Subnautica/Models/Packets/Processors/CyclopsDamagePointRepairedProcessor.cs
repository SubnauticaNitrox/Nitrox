using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    class CyclopsDamagePointRepairedProcessor : AuthenticatedPacketProcessor<CyclopsDamagePointRepaired>
    {
        private readonly PlayerManager playerManager;

        public CyclopsDamagePointRepairedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(CyclopsDamagePointRepaired packet, Player simulatingPlayer)
        {
            playerManager.SendPacketToOtherPlayers(packet, simulatingPlayer);
        }
    }
}
