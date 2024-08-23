using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerInCyclopsMovementProcessor : ClientPacketProcessor<PlayerInCyclopsMovement>
{
    private readonly PlayerManager remotePlayerManager;

    public PlayerInCyclopsMovementProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;
    }

    public override void Process(PlayerInCyclopsMovement movement)
    {
        if (remotePlayerManager.TryFind(movement.PlayerId, out RemotePlayer remotePlayer) && remotePlayer.Pawn != null)
        {
            remotePlayer.UpdatePositionInCyclops(movement.LocalPosition.ToUnity(), movement.LocalRotation.ToUnity());
        }
    }
}
