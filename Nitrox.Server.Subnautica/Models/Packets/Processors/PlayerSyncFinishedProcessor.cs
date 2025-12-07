using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    internal class PlayerSyncFinishedProcessor : AuthenticatedPacketProcessor<PlayerSyncFinished>
    {
        private readonly PlayerManager playerManager;
        private readonly JoiningManager joiningManager;
        private readonly HibernateService hibernateService;
        private readonly ILogger<PlayerSyncFinishedProcessor> logger;

        public PlayerSyncFinishedProcessor(PlayerManager playerManager, JoiningManager joiningManager, HibernateService hibernateService, ILogger<PlayerSyncFinishedProcessor> logger)
        {
            this.playerManager = playerManager;
            this.joiningManager = joiningManager;
            this.hibernateService = hibernateService;
            this.logger = logger;
        }

        public override void Process(PlayerSyncFinished packet, Player player)
        {
            // If this is the first player connecting we need to restart time at this exact moment
            if (playerManager.GetConnectedPlayers().Count == 1)
            {
                hibernateService.WakeAsync().ContinueWithHandleError(ex => logger.ZLogError(ex, $"Error while trying to enter low power mode"));
            }

            joiningManager.SyncFinishedCallback?.Invoke();
        }
    }
}
