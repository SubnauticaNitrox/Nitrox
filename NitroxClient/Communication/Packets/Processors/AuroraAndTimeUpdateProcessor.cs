using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class AuroraAndTimeUpdateProcessor(TimeManager timeManager) : IClientPacketProcessor<AuroraAndTimeUpdate>
{
    private readonly TimeManager timeManager = timeManager;

    public Task Process(ClientProcessorContext context, AuroraAndTimeUpdate packet)
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
