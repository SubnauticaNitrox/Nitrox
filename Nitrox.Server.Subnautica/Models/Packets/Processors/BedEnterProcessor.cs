using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class BedEnterProcessor : AuthenticatedPacketProcessor<BedEnter>
{
    private readonly SleepManager sleepManager;

    public BedEnterProcessor(SleepManager sleepManager)
    {
        this.sleepManager = sleepManager;
    }

    public override void Process(BedEnter packet, Player player)
    {
        sleepManager.PlayerEnteredBed(player);
    }
}
