using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class AuroraExplodeNowProcessor : ClientPacketProcessor<AuroraExplodeNow>
{
    private readonly PDAManagerEntry pdaManagerEntry;

    public AuroraExplodeNowProcessor(PDAManagerEntry pdaManagerEntry)
    {
        this.pdaManagerEntry = pdaManagerEntry;
    }

    public override void Process(AuroraExplodeNow packet)
    {
        pdaManagerEntry.AuroraExplosionTriggered = true;
        CrashedShipExploder main = CrashedShipExploder.main;
        main.timeMonitor.Update(DayNightCycle.main.timePassedAsFloat);
        // Same code as in OnConsoleCommand_explodeship, but we prefixed it so we can't call it like this
        main.timeToStartCountdown = main.timeMonitor.Get() - 25f + 1f;
        main.timeToStartWarning = main.timeToStartCountdown - 1f;
        Log.Debug("Exploding aurora now");
    }
}
