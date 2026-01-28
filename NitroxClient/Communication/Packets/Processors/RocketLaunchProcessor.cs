using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class RocketLaunchProcessor(Rockets rockets) : IClientPacketProcessor<RocketLaunch>
{
    private readonly Rockets rockets = rockets;

    public Task Process(ClientProcessorContext context, RocketLaunch rocketLaunch)
    {
        rockets.RocketLaunch(rocketLaunch.RocketId);
        return Task.CompletedTask;
    }
}
