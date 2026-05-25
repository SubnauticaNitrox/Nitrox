using System.Threading.Tasks;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerPingCreatedProcessor(LocalPlayer localPlayer) : IClientPacketProcessor<PlayerPingCreated>
{
    public Task Process(ClientProcessorContext context, PlayerPingCreated packet)
    {
        if (localPlayer.SessionId.HasValue && packet.SessionId == localPlayer.SessionId.Value)
        {
            return Task.CompletedTask;
        }

        PlayerPingManager.CreateRemotePing(packet.SessionId, packet.PlayerName, packet.Position, packet.PingId);
        
        return Task.CompletedTask;
    }
}
