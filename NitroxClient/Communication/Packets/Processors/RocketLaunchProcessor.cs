using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class RocketLaunchProcessor : IClientPacketProcessor<RocketLaunch>
{
    private readonly Rockets rockets;

    public RocketLaunchProcessor(Rockets rockets)
    {
        this.rockets = rockets;
    }

    public Task Process(IPacketProcessContext context, RocketLaunch rocketLaunch)
    {
        rockets.RocketLaunch(rocketLaunch.RocketId);
        return Task.CompletedTask;
    }
}
