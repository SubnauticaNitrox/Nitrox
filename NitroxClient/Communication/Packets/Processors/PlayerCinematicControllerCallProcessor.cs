using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.BedSync;
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

        // Check if this is a bed - beds need special handling
        if (entity.TryGetComponent(out RemoteBedController bedController))
        {
            if (!playerManager.TryFind(packet.SessionId, out RemotePlayer bedRemotePlayer))
            {
                return Task.CompletedTask;
            }

            // Defensive check for remote player initialization
            if (!bedRemotePlayer.Body || !bedRemotePlayer.Body.activeInHierarchy)
            {
                return Task.CompletedTask;
            }

            if (packet.StartPlaying)
            {
                bedRemotePlayer.InCinematic = true;
                if (bedRemotePlayer.AnimationController)
                {
                    bedRemotePlayer.AnimationController.UpdatePlayerAnimations = false;
                }
                bedController.StartBedAnimation(bedRemotePlayer, packet.Key);
            }
            else
            {
                bedController.EndBedAnimation(bedRemotePlayer, packet.Key);
                bedRemotePlayer.InCinematic = false;
                if (bedRemotePlayer.AnimationController)
                {
                    bedRemotePlayer.AnimationController.UpdatePlayerAnimations = true;
                }
            }

            return Task.CompletedTask;
        }

        // Standard cinematic handling for non-bed objects
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

        if (packet.StartPlaying)
        {
            // Apply animation parameters before starting cinematic
            if (packet.AnimationParameters != null && packet.AnimationParameters.Count > 0)
            {
                ApplyAnimationParameters(reference, packet.Key, packet.ControllerNameHash, remotePlayer, packet.AnimationParameters);
            }
            
            remotePlayer.InCinematic = true;
            remotePlayer.AnimationController.UpdatePlayerAnimations = false;
            reference.CallStartCinematicMode(packet.Key, packet.ControllerNameHash, remotePlayer);
        }
        else
        {
            reference.CallCinematicModeEnd(packet.Key, packet.ControllerNameHash, remotePlayer);
            remotePlayer.InCinematic = false;
            remotePlayer.AnimationController.UpdatePlayerAnimations = true;
        }
        return Task.CompletedTask;
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

    /// <summary>
    /// Gets the PlayerCinematicController from the MultiplayerCinematicReference using reflection.
    /// </summary>
    private static bool TryGetCinematicController(MultiplayerCinematicReference reference, string key, int controllerNameHash, out PlayerCinematicController cinematicController)
    {
        cinematicController = null;

        // Access private controllerByKey field
        var controllerByKeyField = typeof(MultiplayerCinematicReference).GetField("controllerByKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (controllerByKeyField == null)
        {
            return false;
        }

        var controllerByKey = controllerByKeyField.GetValue(reference) as Dictionary<string, Dictionary<int, MultiplayerCinematicController>>;
        if (controllerByKey == null || !controllerByKey.TryGetValue(key, out var controllers) || !controllers.TryGetValue(controllerNameHash, out var multiplayerController))
        {
            return false;
        }

        // Access private playerController field
        var playerControllerField = typeof(MultiplayerCinematicController).GetField("playerController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (playerControllerField == null)
        {
            return false;
        }

        cinematicController = playerControllerField.GetValue(multiplayerController) as PlayerCinematicController;
        return cinematicController != null;
    }
}
