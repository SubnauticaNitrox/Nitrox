using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerMovementProcessor : ClientPacketProcessor<PlayerMovement>
{
    private readonly PlayerManager remotePlayerManager;

    public PlayerMovementProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;
    }

    public override void Process(PlayerMovement movement)
    {
        if (remotePlayerManager.TryFind(movement.PlayerId, out RemotePlayer remotePlayer))
        {
            remotePlayer.UpdatePosition(movement.Position.ToUnity(),
                                        movement.Velocity.ToUnity(),
                                        movement.BodyRotation.ToUnity(),
                                        movement.AimingRotation.ToUnity());
        }
    }
}
