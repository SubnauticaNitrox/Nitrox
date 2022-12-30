using System.Collections;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
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
        Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(movement.PlayerId);
        if (!remotePlayer.HasValue)
        {
            return;
        }

        Multiplayer.Main.StartCoroutine(QueueForFixedUpdate(remotePlayer.Value, movement));
    }

    private IEnumerator QueueForFixedUpdate(RemotePlayer player, PlayerMovement movement)
    {
        yield return Yielders.WaitForFixedUpdate;
        player.UpdatePosition(movement.Position.ToUnity(),
                              movement.Velocity.ToUnity(),
                              movement.BodyRotation.ToUnity(),
                              movement.AimingRotation.ToUnity());
    }
}
