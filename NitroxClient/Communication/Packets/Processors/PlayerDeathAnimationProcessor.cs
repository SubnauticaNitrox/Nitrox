using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerDeathAnimationProcessor(PlayerManager playerManager) : IClientPacketProcessor<PlayerDeathAnimation>
{
    private readonly PlayerManager playerManager = playerManager;

    public Task Process(ClientProcessorContext context, PlayerDeathAnimation packet)
    {
        Optional<RemotePlayer> remotePlayer = playerManager.Find(packet.SessionId);
        if (remotePlayer.HasValue)
        {
            remotePlayer.Value.PlayDeathAnimation(packet.AnimationType);
        }
        return Task.CompletedTask;
    }
}
