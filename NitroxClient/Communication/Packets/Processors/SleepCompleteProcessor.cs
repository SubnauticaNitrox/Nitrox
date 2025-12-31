using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SleepCompleteProcessor : ClientPacketProcessor<SleepComplete>
{
    private readonly SleepManager sleepManager;

    public SleepCompleteProcessor(SleepManager sleepManager)
    {
        this.sleepManager = sleepManager;
    }

    public override void Process(SleepComplete packet)
    {
        sleepManager.OnSleepComplete();
    }
}
