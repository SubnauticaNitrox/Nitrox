using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

/// <summary>
/// Treat any sunbeam state update request from the clients
/// </summary>
public class SunbeamUpdateProcessor : AuthenticatedPacketProcessor<SunbeamUpdate>
{
    private readonly EventTriggerer eventTriggerer;

    public SunbeamUpdateProcessor(EventTriggerer eventTriggerer)
    {
        this.eventTriggerer = eventTriggerer;
    }

    public override void Process(SunbeamUpdate packet, Player player)
    {
        eventTriggerer.SunbeamCountdownStartingTime = packet.CountdownStartingTime;
    }
}
