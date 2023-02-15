using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class AuroraAndTimeUpdateProcessor : ClientPacketProcessor<AuroraAndTimeUpdate>
{
    private readonly TimeManager timeManager;

    public AuroraAndTimeUpdateProcessor(TimeManager timeManager)
    {
        this.timeManager = timeManager;
    }

    public override void Process(AuroraAndTimeUpdate packet)
    {
        timeManager.ProcessUpdate(packet.TimeData.TimePacket);
        StoryManager.UpdateAuroraData(packet.TimeData.AuroraEventData);
        if (packet.Restore)
        {
            StoryManager.RestoreAurora();
        }
    }
}
