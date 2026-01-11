using Nitrox.Model.DataStructures;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using Nitrox.Model.Helper;
using Nitrox.Model.Packets;
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

        Optional<RemotePlayer> opPlayer = playerManager.Find(packet.PlayerId);
        Validate.IsPresent(opPlayer);

        RemotePlayer remotePlayer = opPlayer.Value;

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

            // Handle special cases where cinematic end should trigger state changes
            HandleCinematicEndSideEffects(entity, packet.Key);
        }
    }

    /// <summary>
    /// Some cinematics trigger state changes when they end (e.g., BulkheadDoor toggles open/closed).
    /// This method handles those side effects for remote players.
    /// </summary>
    private static void HandleCinematicEndSideEffects(GameObject entity, string cinematicKey)
    {
        // BulkheadDoor toggles its state when the cinematic ends
        if (entity.TryGetComponent(out BulkheadDoor bulkheadDoor))
        {
            bulkheadDoor.SetState(!bulkheadDoor.opened);
        }
    }
}
