using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class CyclopsDecoyLaunchProcessor(Cyclops cyclops) : IClientPacketProcessor<CyclopsDecoyLaunch>
{
    private readonly Cyclops cyclops = cyclops;

    public Task Process(ClientProcessorContext context, CyclopsDecoyLaunch decoyLaunchPacket)
    {
        cyclops.LaunchDecoy(decoyLaunchPacket.Id);
        return Task.CompletedTask;
    }
}
