using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerInCyclopsMovementProcessor(PlayerManager remotePlayerManager) : IClientPacketProcessor<PlayerInCyclopsMovement>
{
    private readonly PlayerManager remotePlayerManager = remotePlayerManager;

    public Task Process(ClientProcessorContext context, PlayerInCyclopsMovement movement)
    {
        if (remotePlayerManager.TryFind(movement.PlayerId, out RemotePlayer remotePlayer) && remotePlayer.Pawn != null)
        {
            remotePlayer.UpdatePositionInCyclops(movement.LocalPosition.ToUnity(), movement.LocalRotation.ToUnity());
        }
        return Task.CompletedTask;
    }
}
