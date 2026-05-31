using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerMovementProcessor(PlayerManager remotePlayerManager) : IClientPacketProcessor<PlayerMovement>
{
    private readonly PlayerManager remotePlayerManager = remotePlayerManager;

    public Task Process(ClientProcessorContext context, PlayerMovement movement)
    {
        if (remotePlayerManager.TryFind(movement.SessionId, out RemotePlayer remotePlayer))
        {
            remotePlayer.UpdatePosition(movement.Position.ToUnity(),
                                        movement.Velocity.ToUnity(),
                                        movement.BodyRotation.ToUnity(),
                                        movement.AimingRotation.ToUnity());
        }
        return Task.CompletedTask;
    }
}
