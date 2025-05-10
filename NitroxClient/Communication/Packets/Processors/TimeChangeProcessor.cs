using NitroxClient.GameLogic;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class TimeChangeProcessor : IClientPacketProcessor<TimeChange>
{
    private readonly TimeManager timeManager;

    public TimeChangeProcessor(TimeManager timeManager)
    {
        this.timeManager = timeManager;
    }

    public Task Process(IPacketProcessContext context, TimeChange timeChangePacket)
    {
        timeManager.ProcessUpdate(timeChangePacket);
        return Task.CompletedTask;
    }
}
