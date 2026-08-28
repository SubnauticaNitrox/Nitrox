using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerPingCreatedProcessor(LocalPlayer localPlayer) : IClientPacketProcessor<PlayerPingCreated>
{
    public Task Process(ClientProcessorContext context, PlayerPingCreated packet)
    {
        if (localPlayer.SessionId.HasValue && packet.SessionId == localPlayer.SessionId.Value)
        {
            return Task.CompletedTask;
        }

        PlayerPingManager.CreateRemotePing(packet.SessionId, packet.Text, packet.Position, packet.PingId);
        
        return Task.CompletedTask;
    }
}
