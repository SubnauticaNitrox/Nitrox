using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class BedEnterProcessor(SleepManager sleepManager) : IAuthPacketProcessor<BedEnter>
{
    private readonly SleepManager sleepManager = sleepManager;

    public Task Process(AuthProcessorContext context, BedEnter packet)
    {
        sleepManager.PlayerEnteredBed(context.Sender);
        return Task.CompletedTask;
    }
}
