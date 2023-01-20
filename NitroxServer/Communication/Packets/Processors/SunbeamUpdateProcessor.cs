using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

/// <summary>
/// Treat any sunbeam state update request from the clients
/// </summary>
public class SunbeamUpdateProcessor : AuthenticatedPacketProcessor<SunbeamUpdate>
{
    private readonly StoryManager storyManager;

    public SunbeamUpdateProcessor(StoryManager storyManager)
    {
        this.storyManager = storyManager;
    }

    public override void Process(SunbeamUpdate packet, Player player)
    {
        storyManager.SunbeamCountdownStartingTime = packet.CountdownStartingTime;
    }
}
