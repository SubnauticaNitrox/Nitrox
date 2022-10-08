using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class RocketLaunchProcessor : ClientPacketProcessor<RocketLaunch>
{
    private readonly Rockets rockets;

    public RocketLaunchProcessor(Rockets rockets)
    {
        this.rockets = rockets;
    }

    public override void Process(RocketLaunch rocketLaunch)
    {
        rockets.RocketLaunch(rocketLaunch.RocketId);        
    }
}
