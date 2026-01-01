using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    internal sealed class EscapePodChangedPacketProcessor : AuthenticatedPacketProcessor<EscapePodChanged>
    {
        private readonly PlayerManager playerManager;
        private readonly ILogger<EscapePodChangedPacketProcessor> logger;

        public EscapePodChangedPacketProcessor(PlayerManager playerManager, ILogger<EscapePodChangedPacketProcessor> logger)
        {
            this.playerManager = playerManager;
            this.logger = logger;
        }

        public override void Process(EscapePodChanged packet, Player player)
        {
            logger.ZLogDebug($"{packet}");
            player.SubRootId = packet.EscapePodId;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
