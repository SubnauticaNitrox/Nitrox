using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours.Discord;
using NitroxClient.Services.Game;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class DiscordRequestIPProcessor : IClientPacketProcessor<DiscordRequestIP>
{
    public Task Process(ClientProcessorContext context, DiscordRequestIP packet)
    {
        DiscordClientService.UpdateIpPort(packet.IpPort);
        return Task.CompletedTask;
    }
}
