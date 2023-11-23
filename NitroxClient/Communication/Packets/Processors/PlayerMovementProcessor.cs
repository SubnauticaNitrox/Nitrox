using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

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
        Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(movement.Id);
        if (!remotePlayer.HasValue)
        {
            return;
        }

        remotePlayer.Value.UpdatePosition(movement.Position.ToUnity(),
                              movement.Rotation.ToUnity(),
                              movement.AimingRotation.ToUnity());
    }
}
