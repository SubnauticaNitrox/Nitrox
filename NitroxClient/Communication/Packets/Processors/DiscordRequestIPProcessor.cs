using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours.Discord;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class DiscordRequestIPProcessor : ClientPacketProcessor<DiscordRequestIP>
{
    // This is handling for discord rich presence, I think
    public override void Process(DiscordRequestIP packet)
    {
        DiscordClient.UpdateIpPort(packet.IpPort);
    }
}
