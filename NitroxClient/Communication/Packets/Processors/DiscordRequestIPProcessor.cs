using NitroxClient.MonoBehaviours.Discord;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class DiscordRequestIPProcessor : IClientPacketProcessor<DiscordRequestIP>
{
    public Task Process(IPacketProcessContext context, DiscordRequestIP packet)
    {
        DiscordClient.UpdateIpPort(packet.IpPort);

        return Task.CompletedTask;
    }
}
