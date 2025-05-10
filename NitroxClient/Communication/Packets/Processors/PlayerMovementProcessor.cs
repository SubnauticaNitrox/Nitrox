using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerMovementProcessor : IClientPacketProcessor<PlayerMovement>
{
    private readonly PlayerManager remotePlayerManager;

    public PlayerMovementProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;
    }

    public Task Process(IPacketProcessContext context, PlayerMovement movement)
    {
        if (remotePlayerManager.TryFind(movement.PlayerId, out RemotePlayer remotePlayer))
        {
            remotePlayer.UpdatePosition(movement.Position.ToUnity(),
                                        movement.Velocity.ToUnity(),
                                        movement.BodyRotation.ToUnity(),
                                        movement.AimingRotation.ToUnity());
        }

        return Task.CompletedTask;
    }
}
