using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class TimeChangeProcessor : ClientPacketProcessor<TimeChange>
{
    private readonly TimeManager timeManager;

    public TimeChangeProcessor(TimeManager timeManager)
    {
        this.timeManager = timeManager;
    }

    public override void Process(TimeChange timeChangePacket)
    {
        timeManager.ProcessUpdate(timeChangePacket);
    }
}
