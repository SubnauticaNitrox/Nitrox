using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    /// <summary>
    /// This is the absolute damage state. The current simulation owner is the only one who sends this packet to the server
    /// </summary>
    sealed class CyclopsDamageProcessor : AuthenticatedPacketProcessor<CyclopsDamage>
    {
        private readonly PlayerManager playerManager;
        private readonly ILogger<CyclopsDamageProcessor> logger;

        public CyclopsDamageProcessor(PlayerManager playerManager, ILogger<CyclopsDamageProcessor> logger)
        {
            this.playerManager = playerManager;
            this.logger = logger;
        }

        public override void Process(CyclopsDamage packet, Player simulatingPlayer)
        {
            logger.ZLogDebug($"New cyclops damage from player #{simulatingPlayer.Id}: {packet}");

            playerManager.SendPacketToOtherPlayers(packet, simulatingPlayer);
        }
    }
}
