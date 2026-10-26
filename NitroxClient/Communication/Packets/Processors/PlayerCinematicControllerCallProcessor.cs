using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerCinematicControllerCallProcessor(PlayerManager playerManager) : IClientPacketProcessor<PlayerCinematicControllerCall>
{
    private readonly PlayerManager playerManager = playerManager;

    public Task Process(ClientProcessorContext context, PlayerCinematicControllerCall packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.ControllerID, out GameObject entity))
        {
            Log.Warn($"Could not find entity with ID {packet.ControllerID} for cinematic");
            return Task.CompletedTask;
        }

        if (!entity.TryGetComponent(out MultiplayerCinematicReference reference))
        {
            return Task.CompletedTask;
        }

        if (!playerManager.TryFind(packet.SessionId, out RemotePlayer remotePlayer))
        {
            return Task.CompletedTask;
        }

        // Defensive check: Ensure remote player is fully initialized before processing cinematic packets
        if (!remotePlayer.Body || !remotePlayer.Body.activeInHierarchy || !remotePlayer.AnimationController)
        {
            return Task.CompletedTask;
        }

        if (entity.TryGetComponent(out Bed bed))
        {
            ProcessBed(bed, reference, remotePlayer, packet);
            return Task.CompletedTask;
        }

        // Without this the remote player's position moves but their pose never leaves the locomotion blend tree (looks like floating).
        remotePlayer.AnimationController["cinematics_enabled"] = packet.StartPlaying;

        if (packet.StartPlaying)
        {
            // Apply animation parameters before starting cinematic
            if (packet.AnimationParameters != null && packet.AnimationParameters.Count > 0)
            {
                ApplyAnimationParameters(reference, packet.Key, packet.ControllerNameHash, remotePlayer, packet.AnimationParameters);
            }

            remotePlayer.SetInCinematic(packet.ControllerID);
            reference.CallStartCinematicMode(packet.Key, packet.ControllerNameHash, remotePlayer);
        }
        else
        {
            reference.CallCinematicModeEnd(packet.Key, packet.ControllerNameHash, remotePlayer);
            remotePlayer.ClearInCinematic();
        }
        return Task.CompletedTask;
    }

    // Bed's rig is shared by both sides, so the generic path would mean multiple occupants would mess up each other's animator state.
    private static void ProcessBed(Bed bed, MultiplayerCinematicReference reference, RemotePlayer remotePlayer, PlayerCinematicControllerCall packet)
    {
        if (!packet.StartPlaying && !packet.Key.StartsWith("bed_up"))
        {
            return;
        }

        remotePlayer.AnimationController["cinematics_enabled"] = packet.StartPlaying;

        if (!packet.StartPlaying)
        {
            remotePlayer.AnimationController[packet.Key] = false;
            if (bed.gameObject.TryGetComponent(out BedRemotePositioner positioner))
            {
                positioner.End(remotePlayer);
            }
            remotePlayer.ClearInCinematic();
            return;
        }

        if (!bed.gameObject.TryGetComponent(out NitroxEntity bedEntity) ||
            !TryGetCinematicController(reference, packet.Key, packet.ControllerNameHash, out PlayerCinematicController controller))
        {
            return;
        }

        if (packet.Key.StartsWith("bed_up"))
        {
            remotePlayer.AnimationController[packet.Key.Replace("bed_up", "bed_down")] = false;
        }
        remotePlayer.AnimationController[packet.Key] = true;

        remotePlayer.SetInCinematic(BedLockId.Resolve(bedEntity, controller));
        bed.gameObject.EnsureComponent<BedRemotePositioner>().Begin(remotePlayer, controller);
    }

    /// <summary>
    /// Applies animation parameters to the cinematic and player animators.
    /// </summary>
    private static void ApplyAnimationParameters(MultiplayerCinematicReference reference, string key, int controllerNameHash, RemotePlayer remotePlayer, Dictionary<string, bool> animationParameters)
    {
        if (!TryGetCinematicController(reference, key, controllerNameHash, out PlayerCinematicController cinematicController))
        {
            return;
        }

        // Apply parameters to the cinematic animator
        if (cinematicController.animator != null)
        {
            foreach (var param in animationParameters)
            {
                if (param.Key == "first_use" || param.Key == "cured")
                {
                    SafeAnimator.SetBool(cinematicController.animator, param.Key, param.Value);
                }
            }
        }

        // Apply parameters to the remote player's animator
        Animator playerAnimator = remotePlayer.Body.GetComponentInChildren<Animator>();
        if (playerAnimator && playerAnimator.gameObject.activeInHierarchy)
        {
            foreach (var param in animationParameters)
            {
                if (param.Key == "using_tool_first" || param.Key == "cured")
                {
                    SafeAnimator.SetBool(playerAnimator, param.Key, param.Value);
                }
            }
        }
    }

    private static bool TryGetCinematicController(MultiplayerCinematicReference reference, string key, int controllerNameHash, out PlayerCinematicController cinematicController)
    {
        cinematicController = reference.TryGetController(key, controllerNameHash, out MultiplayerCinematicController multiplayerController) ? multiplayerController.PlayerController : null;
        return cinematicController != null;
    }
}
