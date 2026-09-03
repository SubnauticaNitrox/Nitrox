using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class BedExitProcessor(SleepManager sleepManager) : IAuthPacketProcessor<BedExit>
{
    private readonly SleepManager sleepManager = sleepManager;

    public async Task Process(AuthProcessorContext context, BedExit packet)
    {
        await sleepManager.PlayerExitedBed(context.Sender);
    }
}
