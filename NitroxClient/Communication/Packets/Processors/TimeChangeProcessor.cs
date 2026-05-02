using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class TimeChangeProcessor(TimeManager timeManager) : IClientPacketProcessor<TimeChange>
{
    private readonly TimeManager timeManager = timeManager;

    public Task Process(ClientProcessorContext context, TimeChange timeChangePacket)
    {
        timeManager.ProcessUpdate(timeChangePacket);
        return Task.CompletedTask;
    }
}
