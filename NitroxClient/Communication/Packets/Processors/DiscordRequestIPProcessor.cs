using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours.Discord;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class DiscordRequestIPProcessor : ClientPacketProcessor<DiscordRequestIP>
{
    public override void Process(DiscordRequestIP packet)
    {
        DiscordClient.UpdateIpPort(packet.IpPort);
    }
}
