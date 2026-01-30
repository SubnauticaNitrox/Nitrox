using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerSyncFinishedProcessor(SessionManager sessionManager, JoiningManager joiningManager, HibernateService hibernateService)
    : IAuthPacketProcessor<PlayerSyncFinished>
{
    private readonly SessionManager sessionManager = sessionManager;
    private readonly JoiningManager joiningManager = joiningManager;
    private readonly HibernateService hibernateService = hibernateService;

    public async Task Process(AuthProcessorContext context, PlayerSyncFinished packet)
    {
        if (sessionManager.GetSessionCount() > 0)
        {
            await hibernateService.WakeAsync();
        }

        joiningManager.SyncFinishedCallback?.Invoke();
    }
}
