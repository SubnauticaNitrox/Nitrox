using NitroxClient.GameLogic;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class AuroraAndTimeUpdateProcessor : IClientPacketProcessor<AuroraAndTimeUpdate>
{
    private readonly TimeManager timeManager;

    public AuroraAndTimeUpdateProcessor(TimeManager timeManager)
    {
        this.timeManager = timeManager;
    }

    public Task Process(IPacketProcessContext context, AuroraAndTimeUpdate packet)
    {
        timeManager.ProcessUpdate(packet.TimeData.TimePacket);
        StoryManager.UpdateAuroraData(packet.TimeData.AuroraEventData);
        timeManager.AuroraRealExplosionTime = packet.TimeData.AuroraEventData.AuroraRealExplosionTime;
        if (packet.Restore)
        {
            StoryManager.RestoreAurora();
        }
        return Task.CompletedTask;
    }
}
