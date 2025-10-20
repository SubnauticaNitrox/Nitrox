using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    internal class PlayerSyncFinishedProcessor : AuthenticatedPacketProcessor<PlayerSyncFinished>
    {
        private readonly PlayerManager playerManager;
        private readonly Hibernator hibernator;

        public PlayerSyncFinishedProcessor(PlayerManager playerManager, Hibernator hibernator)
        {
            this.playerManager = playerManager;
            this.hibernator = hibernator;
        }

        public override void Process(PlayerSyncFinished packet, Player player)
        {
            // If this is the first player connecting we need to restart time at this exact moment
            if (playerManager.GetConnectedPlayers().Count == 1)
            {
                hibernator.Wake();
            }

            playerManager.FinishProcessingReservation(player);
        }
    }
}
