using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class BedExitProcessor : AuthenticatedPacketProcessor<BedExit>
{
    private readonly SleepManager sleepManager;

    public BedExitProcessor(SleepManager sleepManager)
    {
        this.sleepManager = sleepManager;
    }

    public override void Process(BedExit packet, Player player)
    {
        sleepManager.PlayerExitedBed(player);
    }
}
