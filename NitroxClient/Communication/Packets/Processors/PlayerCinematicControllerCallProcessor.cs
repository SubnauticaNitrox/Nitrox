using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerCinematicControllerCallProcessor : ClientPacketProcessor<PlayerCinematicControllerCall>
{
    private readonly PlayerManager playerManager;

    public PlayerCinematicControllerCallProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(PlayerCinematicControllerCall packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.ControllerID, out GameObject entity))
        {
            return; // Entity can be not spawned yet bc async.
        }

        if (!entity.TryGetComponent(out MultiplayerCinematicReference reference))
        {
            Log.Warn($"Couldn't find {nameof(MultiplayerCinematicReference)} on {entity.name}:{packet.ControllerID}");
            return;
        }

        if (!playerManager.TryFind(packet.PlayerId, out RemotePlayer remotePlayer))
        {
            Log.Warn($"Couldn't find remote player {packet.PlayerId} for cinematic");
            return;
        }

        if (packet.StartPlaying)
        {
            // Set InCinematic flag to prevent movement packets from overriding animation state
            remotePlayer.InCinematic = true;
            remotePlayer.AnimationController.UpdatePlayerAnimations = false;
            reference.CallStartCinematicMode(packet.Key, packet.ControllerNameHash, remotePlayer);
        }
        else
        {
            reference.CallCinematicModeEnd(packet.Key, packet.ControllerNameHash, remotePlayer);
            // Clear InCinematic flag to allow movement packets to control animations again
            remotePlayer.InCinematic = false;
            remotePlayer.AnimationController.UpdatePlayerAnimations = true;
        }
    }
}
