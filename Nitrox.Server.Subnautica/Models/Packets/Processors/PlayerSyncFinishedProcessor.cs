using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    internal class PlayerSyncFinishedProcessor(PlayerManager playerManager, JoiningManager joiningManager, HibernateService hibernateService)
        : IAuthPacketProcessor<PlayerSyncFinished>
    {
        private readonly PlayerManager playerManager = playerManager;
        private readonly JoiningManager joiningManager = joiningManager;
        private readonly HibernateService hibernateService = hibernateService;

        public async Task Process(AuthProcessorContext context, PlayerSyncFinished packet)
        {
            // If this is the first player connecting we need to restart time at this exact moment
            if (playerManager.GetConnectedPlayers().Count == 1)
            {
                await hibernateService.WakeAsync();
            }

            joiningManager.SyncFinishedCallback?.Invoke();
        }
    }
}
