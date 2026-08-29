using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class BedEnterProcessor(SleepManager sleepManager) : IAuthPacketProcessor<BedEnter>
{
    private readonly SleepManager sleepManager = sleepManager;

    public async Task Process(AuthProcessorContext context, BedEnter packet)
    {
        await sleepManager.PlayerEnteredBed(context.Sender);
    }
}
