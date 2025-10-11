using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    public class PlayerSyncFinishedProcessor : AuthenticatedPacketProcessor<PlayerSyncFinished>
    {
        private readonly PlayerManager playerManager;

        public PlayerSyncFinishedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(PlayerSyncFinished packet, Player player)
        {
            // If this is the first player connecting we need to restart time at this exact moment
            if (playerManager.GetConnectedPlayers().Count == 1)
            {
                Server.Instance.ResumeServer();
            }

            playerManager.FinishProcessingReservation(player);
        }
    }
}
